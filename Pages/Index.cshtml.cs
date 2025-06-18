using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AWS_SERVER_CREATOR.Models;
using AWS_SERVER_CREATOR.Services;

namespace AWS_SERVER_CREATOR.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IAwsEc2Service _awsEc2Service;

        public IndexModel(ILogger<IndexModel> logger, IAwsEc2Service awsEc2Service)
        {
            _logger = logger;
            _awsEc2Service = awsEc2Service;
        }

        [BindProperty]
        public new CreateServerRequest Request { get; set; } = new();

        public List<ServerConfiguration> StandardConfigurations { get; set; } = new();
        
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            StandardConfigurations = _awsEc2Service.GetStandardConfigurations();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            StandardConfigurations = _awsEc2Service.GetStandardConfigurations();

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Пожалуйста, заполните все обязательные поля.";
                return Page();
            }

            try
            {
                var configType = HttpContext.Request.Form["configType"].ToString();
                Request.UseStandardConfig = configType == "standard";

                var result = await _awsEc2Service.CreateServerAsync(Request);

                if (result.Success)
                {
                    SuccessMessage = $"Сервер успешно создан! Instance ID: {result.InstanceId}";
                    if (!string.IsNullOrEmpty(result.PublicIpAddress))
                    {
                        SuccessMessage += $", Public IP: {result.PublicIpAddress}";
                    }
                   
                    Request = new CreateServerRequest();
                }
                else
                {
                    ErrorMessage = $"Ошибка при создании сервера: {result.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during server creation");
                ErrorMessage = "Произошла неожиданная ошибка. Пожалуйста, попробуйте еще раз.";
            }

            return Page();
        }
    }
}
