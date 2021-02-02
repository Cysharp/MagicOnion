using Amazon.ECS;
using Amazon.ECS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DFrame.Ecs
{
    internal class EcsService : IDisposable
    {
        private readonly IAmazonECS _client;
        public string ClusterName { get; }
        public string ServiceName { get; }
        public string TaskDefinitionName { get; }
        public string ContainerName { get; }

        public EcsService(string clusterName, string serviceName, string taskDefinitionName, string containerName)
        {
            ClusterName = clusterName;
            ServiceName = serviceName;
            TaskDefinitionName = taskDefinitionName;
            ContainerName = containerName;

            _client = new AmazonECSClient();
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        public async Task<Cluster> GetClusterAsync(CancellationToken ct = default)
        {
            var clusters = await _client.DescribeClustersAsync(new DescribeClustersRequest
            {
                Clusters = new List<string> { ClusterName },
            }, ct).ConfigureAwait(false);
            return clusters.Clusters.FirstOrDefault();
        }
        public async Task<bool> ExistsClusterAsync(CancellationToken ct = default)
        {
            var cluster = await GetClusterAsync(ct).ConfigureAwait(false);
            return cluster != null;
        }

        public async Task<Service> GetServiceAsync(DescribeServicesRequest describeServicesRequest, CancellationToken ct = default)
        {
            var services = await _client.DescribeServicesAsync(describeServicesRequest, ct).ConfigureAwait(false);
            return services.Services.FirstOrDefault();
        }
        public async Task<Service> GetServiceAsync(CancellationToken ct = default)
        {
            var services = await _client.DescribeServicesAsync(new DescribeServicesRequest
            {
                Cluster = ClusterName,
                Services = new List<string> { ServiceName },
            }, ct).ConfigureAwait(false);
            return services.Services.FirstOrDefault();
        }
        public async Task<bool> ExistsServiceAsync(CancellationToken ct = default)
        {
            var service = await GetServiceAsync(ct).ConfigureAwait(false);
            return service != null;
        }

        public async Task<TaskDefinition> GetTaskDefinitionAsync(CancellationToken ct = default)
        {
            var taskDefinition = await _client.DescribeTaskDefinitionAsync(new DescribeTaskDefinitionRequest
            {
                TaskDefinition = TaskDefinitionName,
            }, ct).ConfigureAwait(false);
            return taskDefinition.TaskDefinition;
        }
        public async Task<bool> ExistsTaskDefinitionAsync(CancellationToken ct = default)
        {
            var taskDefinition = await GetTaskDefinitionAsync(ct).ConfigureAwait(false);
            return taskDefinition != null;
        }

        public async Task<NewTaskDefinition> UpdateTaskDefinitionImageAsync(string image, CancellationToken ct = default)
        {
            var taskDefinition = await GetTaskDefinitionAsync(ct).ConfigureAwait(false);
            var containerDefinition = taskDefinition.ContainerDefinitions.FirstOrDefault(x => x.Name == ContainerName);
            containerDefinition.Image = image;
            containerDefinition.Memory = int.Parse(taskDefinition.Memory);
            if (!containerDefinition.Command.Contains("--worker-flag"))
            {
                containerDefinition.Command.Add("--worker-flag");
            }
            var registerResponse = await _client.RegisterTaskDefinitionAsync(new RegisterTaskDefinitionRequest
            {
                ContainerDefinitions = taskDefinition.ContainerDefinitions,
                RequiresCompatibilities = taskDefinition.RequiresCompatibilities,                
                Family = taskDefinition.Family,
                Cpu = taskDefinition.Cpu,
                Memory = taskDefinition.Memory,
                NetworkMode = taskDefinition.NetworkMode,
                Volumes =taskDefinition.Volumes,
                TaskRoleArn = taskDefinition.TaskRoleArn,
                ExecutionRoleArn = taskDefinition.ExecutionRoleArn,
            }, ct).ConfigureAwait(false);

            return new NewTaskDefinition
            {
                TaskRevision = $"{registerResponse.TaskDefinition.Family}:{registerResponse.TaskDefinition.Revision}",
                TaskDefinition = registerResponse.TaskDefinition,
            };
        }

        public async System.Threading.Tasks.Task UpdateServiceDeploymentAsync(string newTaskRevision, int desiredCount, CancellationToken ct = default)
        {
            await PerformServiceDeployment(ClusterName, ServiceName, newTaskRevision, desiredCount, ct).ConfigureAwait(false);
        }
        public async System.Threading.Tasks.Task ScaleServiceAsync(int desiredCount, CancellationToken ct = default)
        {
            await PerformServiceDeployment(ClusterName, ServiceName, desiredCount, ct).ConfigureAwait(false);
        }

        private async Task<bool> PerformServiceDeployment(string cluster, string serviceName, int desiredCount, CancellationToken ct = default)
        {
            var describeServiceRequest = new DescribeServicesRequest
            {
                Cluster = cluster,
                Services = new List<string> { serviceName },
            };
            await _client.UpdateServiceAsync(new UpdateServiceRequest
            {
                Cluster = cluster,
                Service = serviceName,
                DesiredCount = desiredCount
            }, ct).ConfigureAwait(false);
            if (!await WaitTillUpdateServiceComplete(describeServiceRequest, ct).ConfigureAwait(false))
            {
                Console.Error.WriteLine($"ECS Cluster did not start tasks with new task definition for {desiredCount} tasks.");
                return false;
            }

            Console.WriteLine($"Complete deploy {desiredCount} total tasks");
            return true;
        }
        private async Task<bool> PerformServiceDeployment(string cluster, string serviceName, string taskRevision, int desiredCount, CancellationToken ct = default)
        {
            var describeServiceRequest = new DescribeServicesRequest
            {
                Cluster = cluster,
                Services = new List<string> { serviceName },
            };

            Console.WriteLine($"Starting tasks with new revision {taskRevision}.");
            await _client.UpdateServiceAsync(new UpdateServiceRequest
            {
                Cluster = cluster,
                Service = serviceName,
                TaskDefinition = taskRevision,
                DesiredCount = desiredCount,
                ForceNewDeployment = true,                
            }, ct).ConfigureAwait(false);
            if (!await WaitTillUpdateServiceComplete(describeServiceRequest, ct).ConfigureAwait(false))
            {
                Console.Error.WriteLine($"ECS Cluster did not start tasks with new task definition for {desiredCount} tasks.");
                return false;
            }

            Console.WriteLine($"Complete deploy {desiredCount} total tasks");
            return true;
        }

        private async Task<bool> WaitTillUpdateServiceComplete(DescribeServicesRequest describeRequest, CancellationToken ct = default)
        {
            Service service = null;
            do
            {
                await System.Threading.Tasks.Task.Delay(1 * 1000, ct).ConfigureAwait(false);
                service = await GetServiceAsync(describeRequest, ct).ConfigureAwait(false);
            } while (service.Deployments.Count != 1 && !ct.IsCancellationRequested);

            return service.Deployments.Count == 1;
        }
    }

    public class NewTaskDefinition
    {
        public string TaskRevision { get; set; }
        public TaskDefinition TaskDefinition { get; set; }
    }
}
