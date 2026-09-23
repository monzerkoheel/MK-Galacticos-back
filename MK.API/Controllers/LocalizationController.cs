using Microsoft.AspNetCore.Mvc;
using MK.API.Errors;
using MK.Core.HelperModels;
using MK.Core.Localization;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using MK.Service;

namespace MK.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocalizationController : ControllerBase
    {

        [HttpGet("current")]
        public IActionResult GetCurrentLanguage()
        {
            return Ok(new
            {
                message = Enums.ProductGender_Men
            });
        }
    }
}
