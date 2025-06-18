namespace AWS_SERVER_CREATOR.Models
{
    public class AwsConfiguration
    {
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Region { get; set; } = "us-east-1";
    }

    public class ServerConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public string InstanceType { get; set; } = string.Empty;
        public string AmiId { get; set; } = string.Empty;
        public string KeyPairName { get; set; } = string.Empty;
        public string SecurityGroupId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CreateServerRequest
    {
        public AwsConfiguration AwsConfig { get; set; } = new();
        public ServerConfiguration ServerConfig { get; set; } = new();
        public IFormFile? KeyFile { get; set; }
        public bool UseStandardConfig { get; set; }
        public int StandardConfigId { get; set; }
        public string? ExistingKeyName { get; set; }
    }

    public class ServerCreationResult
    {
        public bool Success { get; set; }
        public string InstanceId { get; set; } = string.Empty;
        public string PublicIpAddress { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
