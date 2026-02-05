using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MixxFit.API.DTO.User;
using MixxFit.API.Services.Interfaces;

namespace MixxFit.API.Controllers
{
    [Authorize]
    [Route("api/profile")]
    [ApiController]
    public class ProfileController : BaseController
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<ActionResult<ProfilePageDto>> GetMyProfile(CancellationToken cancellationToken = default)
        {
            return await _profileService.GetUserProfileAsync(CurrentUserId, cancellationToken);
        }
    }
}
