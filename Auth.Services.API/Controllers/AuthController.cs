using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Controllers;
using Welco.Shared.Localization;

namespace Auth.Services.API.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : AppControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var data = new
            {
                Service = "Auth.Services.API",
                Timestamp = DateTime.UtcNow
            };

            return Success(data, Localize(LocalizationKeys.ActionResults.Ok));
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            var healthData = new { Status = "Healthy", Service = "Auth.Services.API" };
            return Success(healthData, Localize(LocalizationKeys.ActionResults.Ok));
        }
    }
}
