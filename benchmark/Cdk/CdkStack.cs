using Amazon.CDK;
using Amazon.CDK.AWS.AppMesh;
using Amazon.CDK.AWS.AutoScaling;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.Ecr.Assets;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Deployment;
using Amazon.CDK.AWS.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cdk
{
    public class CdkStack : Stack
    {
        internal CdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var (now, reportId) = props switch
            {
                ReportStackProps r => (r.ExecuteTime, r.ReportId),
                StackProps _ => (DateTime.Now, Guid.NewGuid().ToString()),
                _ => throw new NotImplementedException(),
            };
            var dframeWorkerLogGroup = "MagicOnionBenchWorkerLogGroup";
            var dframeMasterLogGroup = "MagicOnionBenchMasterLogGroup";

            // s3 deploy
            var s3 = new Bucket(this, "Bucket", new BucketProps
            {
                AutoDeleteObjects = true,
                RemovalPolicy = RemovalPolicy.DESTROY,
                AccessControl = BucketAccessControl.PRIVATE,
                Versioned = true,
            });
            s3.AddToResourcePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Sid = "AllowPublicRead",
                Effect = Effect.ALLOW,
                Principals = new[] { new AnyPrincipal() },
                Actions = new[] { "s3:GetObject*" },
                Resources = new[] { $"{s3.BucketArn}/html/*" },
            }));
            s3.AddToResourcePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Sid = "AllowAwsAccountAccess",
                Effect = Effect.ALLOW,
                Principals = new[] { new AccountRootPrincipal() },
                Actions = new[] { "s3:*" },
                Resources = new[] { $"{s3.BucketArn}/*" },
            }));
            var masterDllDeployment = new BucketDeployment(this, "DeployMasterDll", new BucketDeploymentProps
            {
                DestinationBucket = s3,
                Sources = new[] { Source.Asset(Path.Combine(Directory.GetCurrentDirectory(), "out/linux/server/")) },
                DestinationKeyPrefix = "assembly/linux/server/"
            });

            // docker deploy
            var dockerImage = new DockerImageAsset(this, "dframeWorkerImage", new DockerImageAssetProps
            {
                Directory = Path.Combine(Directory.GetCurrentDirectory(), "app"),
                File = "ConsoleAppEcs/Dockerfile.Ecs",
            });
            var dframeImage = ContainerImage.FromDockerImageAsset(dockerImage);

            // network
            var vpc = new Vpc(this, "Vpc", new VpcProps { MaxAzs = 1, NatGateways = 0, });
            var subnets = new SubnetSelection { Subnets = vpc.PublicSubnets };
            var sg = new SecurityGroup(this, "MasterSg", new SecurityGroupProps
            {
                AllowAllOutbound = true,
                Vpc = vpc,
            });
            foreach (var subnet in vpc.PublicSubnets)
                sg.AddIngressRule(Peer.Ipv4(vpc.VpcCidrBlock), Port.AllTcp(), "VPC", true);

            // service discovery
            var serviceDiscoveryDomain = "local";
            var serverMapName = "server";
            var dframeMapName = "dframe-master";
            var ns = new PrivateDnsNamespace(this, "Namespace", new PrivateDnsNamespaceProps
            {
                Vpc = vpc,
                Name = serviceDiscoveryDomain,
            });
            var serviceDiscoveryServer = ns.CreateService("server", new DnsServiceProps
            {
                Name = serverMapName,
                DnsRecordType = DnsRecordType.A,
                RoutingPolicy = RoutingPolicy.MULTIVALUE,
            });

            // iam
            var iamEc2MagicOnionRole = GetIamEc2MagicOnionRole(s3, serviceDiscoveryServer);
            var iamEcsTaskExecuteRole = GetIamEcsTaskExecuteRole(new[] { dframeWorkerLogGroup, dframeMasterLogGroup });
            var iamDFrameTaskDefRole = GetIamEcsDframeTaskDefRole(s3);
            var iamWorkerTaskDefRole = GetIamEcsWorkerTaskDefRole(s3);

            // MagicOnion
            var asg = new AutoScalingGroup(this, "MagicOnionAsg", new AutoScalingGroupProps
            {
                SpotPrice = "0.01", // 0.0096 for spot price average
                Vpc = vpc,
                SecurityGroup = sg,
                VpcSubnets = subnets,
                InstanceType = InstanceType.Of(InstanceClass.STANDARD3, InstanceSize.MEDIUM),
                DesiredCapacity = 1,
                MaxCapacity = 1,
                MinCapacity = 0,
                AssociatePublicIpAddress = true,
                MachineImage = new AmazonLinuxImage(new AmazonLinuxImageProps
                {
                    CpuType = AmazonLinuxCpuType.X86_64,
                    Generation = AmazonLinuxGeneration.AMAZON_LINUX_2,
                    Storage = AmazonLinuxStorage.GENERAL_PURPOSE,
                    Virtualization = AmazonLinuxVirt.HVM,
                }),
                AllowAllOutbound = true,
                GroupMetrics = new[] { GroupMetrics.All() },
                Role = iamEc2MagicOnionRole,
                UpdatePolicy = UpdatePolicy.ReplacingUpdate(),
                Signals = Signals.WaitForCount(1, new SignalsOptions
                {
                    Timeout = Duration.Minutes(10),
                }),
            });
            asg.AddUserData(@$"#!/bin/bash
# install .NET 5
rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
yum install -y dotnet-sdk-5.0 aspnetcore-runtime-5.0
. /etc/profile.d/dotnet-cli-tools-bin-path.sh
# download server
mkdir -p /var/MagicOnion.Benchmark/server
aws s3 sync --exact-timestamps s3://{s3.BucketName}/assembly/linux/server/ ~/server
chmod +x ~/server/Benchmark.Server
cp -Rf ~/server/ /var/MagicOnion.Benchmark/.
cp -f /var/MagicOnion.Benchmark/server/Benchmark.Server.service /etc/systemd/system/.
systemctl enable Benchmark.Server
systemctl restart Benchmark.Server

# cloudmap
EC2_METADATA=http://169.254.169.254/latest
INSTANCE_ID=$(curl -s $EC2_METADATA/meta-data/instance-id);
INSTANCE_IP=$(curl -s $EC2_METADATA/meta-data/local-ipv4);

sudo cat > /etc/init.d/cloudmap-register <<-EOF
#! /bin/bash -ex
aws servicediscovery register-instance \
    --region {this.Region} \
    --service-id {serviceDiscoveryServer.ServiceId} \
    --instance-id $INSTANCE_ID \
    --attributes AWS_INSTANCE_IPV4=$INSTANCE_IP,AWS_INSTANCE_PORT=80
exit 0
EOF
chmod a+x /etc/init.d/cloudmap-register

sudo cat > /etc/init.d/cloudmap-deregister <<-EOF
#! /bin/bash -ex
aws servicediscovery deregister-instance \
 --region {this.Region} \
 --service-id {serviceDiscoveryServer.ServiceId} \
 --instance-id $INSTANCE_ID
exit 0
EOF
chmod a+x /etc/init.d/cloudmap-deregister

cat > /usr/lib/systemd/system/cloudmap.service <<-EOF
[Unit]
Description=Run CloudMap service
Requires=network-online.target network.target
DefaultDependencies=no
Before=shutdown.target reboot.target halt.target

[Service]
Type=oneshot
KillMode=none
RemainAfterExit=yes
ExecStart=/etc/init.d/cloudmap-register
ExecStop=/etc/init.d/cloudmap-deregister

[Install]
WantedBy=multi-user.target
EOF

systemctl enable cloudmap.service
systemctl start  cloudmap.service

".Replace("\r\n", "\n"));
            asg.UserData.AddSignalOnExitCommand(asg);
            asg.Node.AddDependency(masterDllDeployment);
            // could not roll back to desired count = 1
            new CfnScheduledAction(this, "ScheduleOut", new CfnScheduledActionProps
            {
                AutoScalingGroupName = asg.AutoScalingGroupName,
                DesiredCapacity = 1,
                MaxSize = 1,
                Recurrence = "0 0 * 1-5 *", // AM9:00 (JST+9)
            });
            new CfnScheduledAction(this, "ScheduleIn", new CfnScheduledActionProps
            {
                AutoScalingGroupName = asg.AutoScalingGroupName,
                DesiredCapacity = 0,
                MaxSize = 0,
                Recurrence = "0 12 * 1-5 *", // PM21:00 (JST+9)
            });

            // ECS
            var cluster = new Cluster(this, "WorkerCluster", new ClusterProps
            {
                Vpc = vpc,
            });
            cluster.Node.AddDependency(asg); // wait until asg is up

            // dframe-worker
            var dframeWorkerContainerName = "worker";
            var dframeWorkerTaskDef = new FargateTaskDefinition(this, "DFrameWorkerTaskDef", new FargateTaskDefinitionProps
            {
                ExecutionRole = iamEcsTaskExecuteRole,
                TaskRole = iamWorkerTaskDefRole,
                Cpu = 256,
                MemoryLimitMiB = 512,
            });
            dframeWorkerTaskDef.AddContainer("worker", new ContainerDefinitionOptions
            {
                Image = dframeImage,
                Command = new[] { "--worker-flag" },
                Environment = new Dictionary<string, string>
                {
                    { "DFRAME_MASTER_CONNECT_TO_HOST", $"{dframeMapName}.{serviceDiscoveryDomain}"},
                    { "DFRAME_MASTER_CONNECT_TO_PORT", "12345"},
                    { "BENCH_SERVER_HOST", $"http://{serverMapName}.{serviceDiscoveryDomain}" },
                    { "BENCH_REPORTID", reportId },
                    { "BENCH_S3BUCKET", s3.BucketName },
                },
                Logging = LogDriver.AwsLogs(new AwsLogDriverProps
                {
                    LogGroup = new LogGroup(this, "WorkerLogGroup", new LogGroupProps
                    {
                        LogGroupName = dframeWorkerLogGroup,
                        RemovalPolicy = RemovalPolicy.DESTROY,
                        Retention = RetentionDays.TWO_WEEKS,
                    }),
                    StreamPrefix = dframeWorkerLogGroup,
                }),
            });
            var dframeWorkerService = new FargateService(this, "DFrameWorkerService", new FargateServiceProps
            {
                DesiredCount = 0,
                Cluster = cluster,
                TaskDefinition = dframeWorkerTaskDef,
                VpcSubnets = subnets,
                SecurityGroups = new[] { sg },
                PlatformVersion = FargatePlatformVersion.VERSION1_4,
                MinHealthyPercent = 0,
                AssignPublicIp = true,
            });

            // dframe-master
            var dframeMasterTaskDef = new FargateTaskDefinition(this, "DFrameMasterTaskDef", new FargateTaskDefinitionProps
            {
                ExecutionRole = iamEcsTaskExecuteRole,
                TaskRole = iamDFrameTaskDefRole,
                Cpu = 256,
                MemoryLimitMiB = 512,
            });
            dframeMasterTaskDef.AddContainer("dframe", new ContainerDefinitionOptions
            {
                Image = dframeImage,                
                Environment = new Dictionary<string, string>
                {
                    { "DFRAME_CLUSTER_NAME", cluster.ClusterName },
                    { "DFRAME_MASTER_SERVICE_NAME", "DFrameMasterService" },
                    { "DFRAME_WORKER_CONTAINER_NAME", dframeWorkerContainerName },
                    { "DFRAME_WORKER_SERVICE_NAME", dframeWorkerService.ServiceName },
                    { "DFRAME_WORKER_TASK_NAME", Fn.Select(1, Fn.Split("/", dframeWorkerTaskDef.TaskDefinitionArn)) },
                    { "DFRAME_WORKER_IMAGE", dockerImage.ImageUri },
                    { "BENCH_REPORTID", reportId },
                    { "BENCH_S3BUCKET", s3.BucketName },
                },
                Logging = LogDriver.AwsLogs(new AwsLogDriverProps
                {
                    LogGroup = new LogGroup(this, "MasterLogGroup", new LogGroupProps
                    {
                        LogGroupName = dframeMasterLogGroup,
                        RemovalPolicy = RemovalPolicy.DESTROY,
                        Retention = RetentionDays.TWO_WEEKS,
                    }),
                    StreamPrefix = dframeMasterLogGroup,
                }),
            });
            var dframeMasterService = new FargateService(this, "DFrameMasterService", new FargateServiceProps
            {
                ServiceName = "DFrameMasterService",
                DesiredCount = 1,
                Cluster = cluster,
                TaskDefinition = dframeMasterTaskDef,
                VpcSubnets = subnets,
                SecurityGroups = new[] { sg },
                PlatformVersion = FargatePlatformVersion.VERSION1_4,
                MinHealthyPercent = 0,
                AssignPublicIp = true,
            });
            dframeMasterService.EnableCloudMap(new CloudMapOptions
            {
                CloudMapNamespace = ns,
                Name = dframeMapName,
                DnsRecordType = DnsRecordType.A,
                DnsTtl = Duration.Seconds(300),
            });

            // output
            var masterTaskFamilyRevision = Fn.Select(1, Fn.Split("/", dframeMasterService.TaskDefinition.TaskDefinitionArn));
            var workerTaskFamilyRevision = Fn.Select(1, Fn.Split("/", dframeWorkerService.TaskDefinition.TaskDefinitionArn));
            new CfnOutput(this, "ReportUrl", new CfnOutputProps { Value = $"https://{s3.BucketRegionalDomainName}/html/{reportId}/index.html" });
            new CfnOutput(this, "EcsClusterName", new CfnOutputProps { Value = cluster.ClusterName });
            new CfnOutput(this, "DFrameMasterEcsServiceName", new CfnOutputProps { Value = dframeMasterService.ServiceName });
            new CfnOutput(this, "DFrameMasterEcsTaskdefArn", new CfnOutputProps { Value = dframeMasterService.TaskDefinition.TaskDefinitionArn });
            new CfnOutput(this, "DFrameMasterEcsTaskdefName", new CfnOutputProps { Value = Fn.Select(0, Fn.Split(":", masterTaskFamilyRevision)) });
            new CfnOutput(this, "DFrameWorkerEcsServiceName", new CfnOutputProps { Value = dframeWorkerService.ServiceName });
            new CfnOutput(this, "DFrameWorkerEcsTaskdefArn", new CfnOutputProps { Value = dframeWorkerService.TaskDefinition.TaskDefinitionArn });
            new CfnOutput(this, "DFrameWorkerEcsTaskdefName", new CfnOutputProps { Value = Fn.Select(0, Fn.Split(":", workerTaskFamilyRevision)) });
            new CfnOutput(this, "DFrameWorkerEcsTaskdefImage", new CfnOutputProps { Value = dockerImage.ImageUri });
        }

        private Role GetIamEc2MagicOnionRole(Bucket s3, Service meshService)
        {
            var policy = new Policy(this, "MasterPolicy", new PolicyProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] { "s3:ListAllMyBuckets" },
                        Resources = new[] { "arn:aws:s3:::*" },
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] { "s3:ListBucket","s3:GetBucketLocation" },
                        Resources = new[] { s3.BucketArn },
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] { "s3:GetObject" },
                        Resources = new[] { $"{s3.BucketArn}/*" },
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] { "servicediscovery:RegisterInstance", "servicediscovery:DeregisterInstance"},
                        Resources = new[] { meshService.ServiceArn },
                    }),
                }
            });
            var role = new Role(this, "MasterRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ec2.amazonaws.com"),
            });
            role.AttachInlinePolicy(policy);
            role.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AmazonSSMManagedInstanceCore"));
            return role;
        }
        private Role GetIamEcsTaskExecuteRole(string[] logGroups)
        {
            var policy = new Policy(this, "WorkerTaskDefExecutionPolicy", new PolicyProps
            {
                Statements = new[]
                {
                    // s3
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[]
                        {
                            "logs:CreateLogStream",
                            "logs:PutLogEvents"
                        },
                        Resources = logGroups.Select(x => $"arn:aws:logs:{this.Region}:{this.Account}:log-group:{x}:*").ToArray(),
                    }),
                }
            });
            var role = new Role(this, "WorkerTaskDefExecutionRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com"),
            });
            role.AttachInlinePolicy(policy);
            role.AddManagedPolicy(ManagedPolicy.FromManagedPolicyArn(this, "WorkerECSTaskExecutionRolePolicy", "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"));
            return role;
        }

        private Role GetIamEcsDframeTaskDefRole(Bucket s3)
        {
            var policy = new Policy(this, "DframeTaskDefTaskPolicy", new PolicyProps
            {
                Statements = new[]
                {
                    // ecs
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] 
                        {
                            "ecs:Describe*",
                            "ecs:List*",
                            "ecs:Update*",
                            "ecs:DiscoverPollEndpoint",
                            "ecs:Poll",
                            "ecs:RegisterContainerInstance",
                            "ecs:RegisterTaskDefinition",
                            "ecs:StartTelemetrySession",
                            "ecs:UpdateContainerInstancesState",
                            "ecs:Submit*",
                        },
                        Resources = new[] { "*" },
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new []
                        {
                            "iam:PassRole",
                        },
                        Resources = new [] { "*" },
                    }),
                    // s3
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] { "s3:ListAllMyBuckets" },
                        Resources = new[] { "arn:aws:s3:::*" },
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] { "s3:ListBucket","s3:GetBucketLocation" },
                        Resources = new[] { s3.BucketArn },
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[]
                        {
                            "s3:PutObject",
                            "s3:PutObjectAcl",
                            "s3:GetObject",
                            "s3:GetObjectAcl",
                        },
                        Resources = new[] { $"{s3.BucketArn}/*" },
                    }),
                }
            });
            var role = new Role(this, "DframeTaskDefTaskRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com"),
            });
            role.AttachInlinePolicy(policy);
            return role;
        }
        private Role GetIamEcsWorkerTaskDefRole(Bucket s3)
        {
            var policy = new Policy(this, "WorkerTaskDefTaskPolicy", new PolicyProps
            {
                Statements = new[]
                {
                    // s3
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] { "s3:ListAllMyBuckets" },
                        Resources = new[] { "arn:aws:s3:::*" },
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] { "s3:ListBucket","s3:GetBucketLocation" },
                        Resources = new[] { s3.BucketArn },
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[]
                        {
                            "s3:PutObject",
                            "s3:PutObjectAcl",
                            "s3:GetObject",
                            "s3:GetObjectAcl",
                        },
                        Resources = new[] { $"{s3.BucketArn}/*" },
                    }),
                }
            });
            var role = new Role(this, "WorkerTaskDefTaskRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com"),
            });
            role.AttachInlinePolicy(policy);
            return role;
        }
    }
}
