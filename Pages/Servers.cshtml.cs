using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AWS_SERVER_CREATOR.Models;
using AWS_SERVER_CREATOR.Services;

namespace AWS_SERVER_CREATOR.Pages
{
    public class ServerInfo
    {
        public string InstanceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string InstanceType { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PublicIpAddress { get; set; } = string.Empty;
        public string PrivateIpAddress { get; set; } = string.Empty;
        public DateTime LaunchTime { get; set; }
    }

    public class ServersModel : PageModel
    {
        private readonly ILogger<ServersModel> _logger;
        private readonly IAwsEc2Service _awsEc2Service;

        public ServersModel(ILogger<ServersModel> logger, IAwsEc2Service awsEc2Service)
        {
            _logger = logger;
            _awsEc2Service = awsEc2Service;
        }

        [BindProperty]
        public AwsConfiguration AwsConfig { get; set; } = new();

        [BindProperty]
        public IFormFile? PemFile { get; set; }


        public List<ServerInfo> Servers { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool ServersLoaded { get; set; } = false;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Пожалуйста, заполните все обязательные поля.";
                return Page();
            }

            if (PemFile != null && PemFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, PemFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await PemFile.CopyToAsync(stream);
                }
            }

            try
            {
                Servers = await LoadServersAsync();
                ServersLoaded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading servers");
                ErrorMessage = $"Ошибка при загрузке серверов: {ex.Message}";
            }

            return Page();
        }



        private async Task<List<ServerInfo>> LoadServersAsync()
        {
            var config = new Amazon.EC2.AmazonEC2Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(AwsConfig.Region)
            };

            using var ec2Client = new Amazon.EC2.AmazonEC2Client(
                AwsConfig.AccessKey,
                AwsConfig.SecretKey,
                config);

            var request = new Amazon.EC2.Model.DescribeInstancesRequest();
            var response = await ec2Client.DescribeInstancesAsync(request);

            var servers = new List<ServerInfo>();

            foreach (var reservation in response.Reservations)
            {
                foreach (var instance in reservation.Instances)
                {
                    var nameTag = instance.Tags?.FirstOrDefault(t => t.Key == "Name");
                    
                    servers.Add(new ServerInfo
                    {
                        InstanceId = instance.InstanceId,
                        Name = nameTag?.Value ?? "Unnamed",
                        InstanceType = instance.InstanceType.Value,
                        State = instance.State.Name.Value,
                        PublicIpAddress = instance.PublicIpAddress ?? string.Empty,
                        PrivateIpAddress = instance.PrivateIpAddress ?? string.Empty,
                        LaunchTime = instance.LaunchTime
                    });
                }
            }

            return servers.OrderByDescending(s => s.LaunchTime).ToList();
        }
    }
}
