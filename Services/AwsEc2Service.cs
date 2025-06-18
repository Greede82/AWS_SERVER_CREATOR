using Amazon.EC2;
using Amazon.EC2.Model;
using AWS_SERVER_CREATOR.Models;

namespace AWS_SERVER_CREATOR.Services
{
    public interface IAwsEc2Service
    {
        Task<ServerCreationResult> CreateServerAsync(CreateServerRequest request);
        List<ServerConfiguration> GetStandardConfigurations();
    }

    public class AwsEc2Service : IAwsEc2Service
    {
        private readonly ILogger<AwsEc2Service> _logger;

        public AwsEc2Service(ILogger<AwsEc2Service> logger)
        {
            _logger = logger;
        }

        public List<ServerConfiguration> GetStandardConfigurations()
        {
            return new List<ServerConfiguration>
            {
                new ServerConfiguration
                {
                    Name = "Micro Web Server",
                    InstanceType = "t2.micro",
                    AmiId = "ami-0c55b159cbfafe1d0", 
                    Description = "Базовый веб-сервер для небольших приложений"
                },
                new ServerConfiguration
                {
                    Name = "Small Development Server",
                    InstanceType = "t3.small",
                    AmiId = "ami-0c55b159cbfafe1d0",
                    Description = "Сервер для разработки и тестирования"
                },
                new ServerConfiguration
                {
                    Name = "Medium Production Server",
                    InstanceType = "t3.medium",
                    AmiId = "ami-0c55b159cbfafe1d0",
                    Description = "Продакшн сервер для средних нагрузок"
                },
                new ServerConfiguration
                {
                    Name = "Large Database Server",
                    InstanceType = "t3.large",
                    AmiId = "ami-0c55b159cbfafe1d0",
                    Description = "Мощный сервер для баз данных"
                },
                new ServerConfiguration
                {
                    Name = "GPU Computing Instance",
                    InstanceType = "p3.2xlarge",
                    AmiId = "ami-0c55b159cbfafe1d0",
                    Description = "Сервер с GPU для машинного обучения"
                }
            };
        }

        public async Task<ServerCreationResult> CreateServerAsync(CreateServerRequest request)
        {
            try
            {
                var config = new AmazonEC2Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(request.AwsConfig.Region)
                };

                using var ec2Client = new AmazonEC2Client(
                    request.AwsConfig.AccessKey,
                    request.AwsConfig.SecretKey,
                    config);

                var serverConfig = request.UseStandardConfig
                    ? GetStandardConfigurations()[request.StandardConfigId]
                    : request.ServerConfig;

                string keyPairName = serverConfig.KeyPairName;
                if (request.KeyFile != null)
                {
                    keyPairName = await CreateKeyPairFromFileAsync(ec2Client, request.KeyFile);
                }

                var securityGroupId = await CreateSecurityGroupAsync(ec2Client, serverConfig.Name);

                var runRequest = new RunInstancesRequest
                {
                    ImageId = serverConfig.AmiId,
                    MinCount = 1,
                    MaxCount = 1,
                    InstanceType = serverConfig.InstanceType,
                    KeyName = keyPairName,
                    SecurityGroupIds = new List<string> { securityGroupId }
                };

                var runResponse = await ec2Client.RunInstancesAsync(runRequest);
                var instance = runResponse.Reservation?.Instances?.FirstOrDefault();

                if (instance == null)
                {
                    return new ServerCreationResult
                    {
                        Success = false,
                        ErrorMessage = "Не удалось создать инстанс"
                    };
                }

                await WaitForInstanceRunningAsync(ec2Client, instance.InstanceId);

                var describeRequest = new DescribeInstancesRequest
                {
                    InstanceIds = new List<string> { instance.InstanceId }
                };

                var describeResponse = await ec2Client.DescribeInstancesAsync(describeRequest);
                var runningInstance = describeResponse.Reservations
                    .SelectMany(r => r.Instances)
                    .FirstOrDefault();

                return new ServerCreationResult
                {
                    Success = true,
                    InstanceId = instance.InstanceId,
                    PublicIpAddress = runningInstance?.PublicIpAddress ?? "Не назначен"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании сервера");
                return new ServerCreationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private async Task<string> CreateKeyPairFromFileAsync(AmazonEC2Client ec2Client, IFormFile keyFile)
        {
            var keyPairName = $"uploaded-key-{DateTime.Now:yyyyMMddHHmmss}";
            
            using var reader = new StreamReader(keyFile.OpenReadStream());
            var publicKeyMaterial = await reader.ReadToEndAsync();

            var importRequest = new ImportKeyPairRequest
            {
                KeyName = keyPairName,
                PublicKeyMaterial = publicKeyMaterial
            };

            await ec2Client.ImportKeyPairAsync(importRequest);
            return keyPairName;
        }

        private async Task<string> CreateSecurityGroupAsync(AmazonEC2Client ec2Client, string serverName)
        {
            var groupName = $"sg-{serverName.Replace(" ", "-").ToLower()}-{DateTime.Now:yyyyMMddHHmmss}";
            
            var createSgRequest = new CreateSecurityGroupRequest
            {
                GroupName = groupName,
                Description = $"Security group for {serverName}"
            };

            var createSgResponse = await ec2Client.CreateSecurityGroupAsync(createSgRequest);

            var authorizeRequest = new AuthorizeSecurityGroupIngressRequest
            {
                GroupId = createSgResponse.GroupId,
                IpPermissions = new List<IpPermission>
                {
                    new IpPermission
                    {
                        IpProtocol = "tcp",
                        FromPort = 22,
                        ToPort = 22,
                        Ipv4Ranges = new List<IpRange> { new IpRange { CidrIp = "0.0.0.0/0" } }
                    },
                    new IpPermission
                    {
                        IpProtocol = "tcp",
                        FromPort = 80,
                        ToPort = 80,
                        Ipv4Ranges = new List<IpRange> { new IpRange { CidrIp = "0.0.0.0/0" } }
                    },
                    new IpPermission
                    {
                        IpProtocol = "tcp",
                        FromPort = 443,
                        ToPort = 443,
                        Ipv4Ranges = new List<IpRange> { new IpRange { CidrIp = "0.0.0.0/0" } }
                    }
                }
            };

            await ec2Client.AuthorizeSecurityGroupIngressAsync(authorizeRequest);
            return createSgResponse.GroupId;
        }

        private async Task WaitForInstanceRunningAsync(AmazonEC2Client ec2Client, string instanceId)
        {
            var maxWaitTime = TimeSpan.FromMinutes(5);
            var startTime = DateTime.Now;

            while (DateTime.Now - startTime < maxWaitTime)
            {
                var describeRequest = new DescribeInstancesRequest
                {
                    InstanceIds = new List<string> { instanceId }
                };

                var response = await ec2Client.DescribeInstancesAsync(describeRequest);
                var instance = response.Reservations
                    .SelectMany(r => r.Instances)
                    .FirstOrDefault();

                if (instance?.State.Name == "running")
                {
                    break;
                }

                await Task.Delay(10000); 
            }
        }
    }
}
