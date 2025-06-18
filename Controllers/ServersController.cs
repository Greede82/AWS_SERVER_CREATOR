using Microsoft.AspNetCore.Mvc;
using AWS_SERVER_CREATOR.Models;
using Amazon.EC2;
using Amazon.EC2.Model;

namespace AWS_SERVER_CREATOR.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServersController : ControllerBase
    {
        private readonly ILogger<ServersController> _logger;

        public ServersController(ILogger<ServersController> logger)
        {
            _logger = logger;
        }

        [HttpPost("stop")]
        public async Task<IActionResult> StopInstance([FromBody] ServerActionRequest request)
        {
            try
            {
                var config = new AmazonEC2Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(request.Region)
                };

                using var ec2Client = new AmazonEC2Client(
                    request.AccessKey,
                    request.SecretKey,
                    config);

                var stopRequest = new StopInstancesRequest
                {
                    InstanceIds = new List<string> { request.InstanceId }
                };

                var response = await ec2Client.StopInstancesAsync(stopRequest);
                
                return Ok(new { success = true, message = "Instance stop initiated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping instance {InstanceId}", request.InstanceId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartInstance([FromBody] ServerActionRequest request)
        {
            try
            {
                var config = new AmazonEC2Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(request.Region)
                };

                using var ec2Client = new AmazonEC2Client(
                    request.AccessKey,
                    request.SecretKey,
                    config);

                var startRequest = new StartInstancesRequest
                {
                    InstanceIds = new List<string> { request.InstanceId }
                };

                var response = await ec2Client.StartInstancesAsync(startRequest);
                
                return Ok(new { success = true, message = "Instance start initiated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting instance {InstanceId}", request.InstanceId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("terminate")]
        public async Task<IActionResult> TerminateInstance([FromBody] ServerActionRequest request)
        {
            try
            {
                var config = new AmazonEC2Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(request.Region)
                };

                using var ec2Client = new AmazonEC2Client(
                    request.AccessKey,
                    request.SecretKey,
                    config);

                var terminateRequest = new TerminateInstancesRequest
                {
                    InstanceIds = new List<string> { request.InstanceId }
                };

                var response = await ec2Client.TerminateInstancesAsync(terminateRequest);
                
                return Ok(new { success = true, message = "Instance termination initiated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error terminating instance {InstanceId}", request.InstanceId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
