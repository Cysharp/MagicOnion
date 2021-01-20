using System;
using System.Threading;
using System.Threading.Tasks;

namespace DFrame.Ecs
{
    public class EcsEnvironment
    {
        /// <summary>
        /// Worker ECS cluster name.
        /// </summary>
        public string ClusterName { get; set; } = Environment.GetEnvironmentVariable("DFRAME_WORKER_CLUSTER_NAME") ?? "dframe-worker-cluster";
        /// <summary>
        /// Worker ECS service name.
        /// </summary>
        public string WorkerServiceName { get; set; } = Environment.GetEnvironmentVariable("DFRAME_WORKER_SERVICE_NAME") ?? "dframe-worker-service";
        /// <summary>
        /// Master ECS service name.
        /// </summary>
        public string MasterServiceName { get; set; } = Environment.GetEnvironmentVariable("DFRAME_MASTER_SERVICE_NAME") ?? "dframe-master-service";
        /// <summary>
        /// Worker ECS task name.
        /// </summary>
        public string TaskDefinitionName { get; set; } = Environment.GetEnvironmentVariable("DFRAME_WORKER_TASK_NAME") ?? "dframe-worker-task";
        /// <summary>
        /// Worker ECS task container name.
        /// </summary>
        public string ContainerName { get; set; } = Environment.GetEnvironmentVariable("DFRAME_WORKER_CONTAINER_NAME") ?? "worker";
        /// <summary>
        /// Image Tag for Worker Image.
        /// </summary>
        public string Image { get; set; } = Environment.GetEnvironmentVariable("DFRAME_WORKER_IMAGE") ?? "";
        /// <summary>
        /// Wait worker task creationg timeout seconds. default 120 sec.
        /// </summary>
        public int WorkerTaskCreationTimeout { get; set; } = int.Parse(Environment.GetEnvironmentVariable("DFRAME_WORKER_POD_CREATE_TIMEOUT") ?? "120");
        /// <summary>
        /// Preserve Worker ECS Service after execution. default false.
        /// </summary>
        /// <remarks>
        /// any value => true
        /// null => false
        /// </remarks>
        public bool PreserveWorker { get; set; } = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DFRAME_WORKER_PRESERVE"));
    }

    public class EcsScalingProvider : IScalingProvider
    {
        IFailSignal _failSignal = default!;

        private readonly EcsEnvironment _env;
        private readonly EcsService _ecsWorker;
        private readonly EcsService _ecsMaster;

        public EcsScalingProvider() : this(new EcsEnvironment())
        {
        }
        public EcsScalingProvider(EcsEnvironment env)
        {
            _env = env;
            _ecsWorker = new EcsService(_env.ClusterName, _env.WorkerServiceName, _env.TaskDefinitionName, _env.ContainerName);
            _ecsMaster = new EcsService(_env.ClusterName, _env.MasterServiceName, "", _env.ContainerName);
        }

        public async Task StartWorkerAsync(DFrameOptions options, int processCount, IServiceProvider provider, IFailSignal failSignal, CancellationToken cancellationToken)
        {
            _failSignal = failSignal;

            Console.WriteLine($"scale out {processCount} workers. Cluster: {_ecsWorker.ClusterName}, Service: {_ecsWorker.ServiceName}, TaskDef: {_ecsWorker.TaskDefinitionName}");

            Console.WriteLine($"checking ECS is ready.");
            if (!await _ecsWorker.ExistsClusterAsync())
            {
                _failSignal.TrySetException(new EcsException($"ECS Cluster not found. Please confirm provided name {nameof(_ecsWorker.ClusterName)} is valid."));
                return;
            }
            if (!await _ecsWorker.ExistsServiceAsync())
            {
                _failSignal.TrySetException(new EcsException($"ECS Service not found in ECS Cluster {_ecsWorker.ClusterName}. Please confirm provided name {nameof(_ecsWorker.ServiceName)} is valid."));
                return;
            }
            if (!await _ecsWorker.ExistsTaskDefinitionAsync())
            {
                _failSignal.TrySetException(new EcsException($"ECS TaskDefinition not found. Please confirm provided name {nameof(_ecsWorker.TaskDefinitionName)} is valid."));
                return;
            }
            if (!await _ecsMaster.ExistsServiceAsync())
            {
                _failSignal.TrySetException(new EcsException($"ECS Service not found in ECS Cluster {_ecsMaster.ClusterName}. Please confirm provided name {nameof(_ecsMaster.ServiceName)} is valid."));
                return;
            }

            using (var cts = new CancellationTokenSource(_env.WorkerTaskCreationTimeout * 1000))
            {
                // create task for desired parameter
                var updatedTaskDefinition = await _ecsWorker.UpdateTaskDefinitionImageAsync(_env.Image);

                // update service and deploy new task
                await _ecsWorker.UpdateServiceDeploymentAsync(updatedTaskDefinition.TaskRevision, processCount);
            }
        }

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine($"scale in workers. Cluster: {_ecsWorker.ClusterName}, Service: {_ecsWorker.ServiceName}, TaskDef: {_ecsWorker.TaskDefinitionName}");
            if (!_env.PreserveWorker)
            {
                using var cts = new CancellationTokenSource(120 * 1000);
                await _ecsWorker.ScaleServiceAsync(0, cts.Token);

                if (!string.IsNullOrEmpty(_env.MasterServiceName))
                {
                    Console.WriteLine($"scale in master. Cluster: {_ecsMaster.ClusterName}, Service: {_ecsMaster.ServiceName}");
                    await _ecsMaster.ScaleServiceAsync(0, cts.Token);
                }
            }
            else
            {
                Console.WriteLine($"detected preserve worker, scale in action skipped.");
            }
        }
    }
}
