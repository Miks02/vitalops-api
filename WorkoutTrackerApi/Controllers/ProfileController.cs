using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkoutTrackerApi.DTO.User;
using WorkoutTrackerApi.Services.Interfaces;

namespace WorkoutTrackerApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<ActionResult<ProfilePageDto>> GetMyProfile(CancellationToken cancellationToken = default)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            return await _profileService.GetUserProfileAsync(userId!, cancellationToken);
        }
    }
}
