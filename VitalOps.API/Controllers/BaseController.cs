using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace VitalOps.API.Controllers
{
    public class BaseController : ControllerBase
    {
        public string CurrentUserId 
            => User.FindFirstValue(ClaimTypes.NameIdentifier) 
               ?? throw new ArgumentNullException(nameof(CurrentUserId), "CRITICAL ERROR: User is null");
    }
}
