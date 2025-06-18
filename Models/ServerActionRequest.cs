namespace AWS_SERVER_CREATOR.Models
{
    public class ServerActionRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }
}
