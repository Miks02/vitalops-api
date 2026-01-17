using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using WorkoutTrackerApi.DTO.Global;
using WorkoutTrackerApi.DTO.User;
using WorkoutTrackerApi.Enums;
using WorkoutTrackerApi.Extensions;
using WorkoutTrackerApi.Services.Interfaces;

namespace WorkoutTrackerApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserDetailsDto>>> GetMe()
        {
            var user = await _userService.GetUserDetailsAsync(GetUserId());

            return ApiResponse<UserDetailsDto>.Success("User fetched successfully", user);
        }
             
        [HttpPatch("fullname")]
        public async Task<ActionResult<string>> UpdateFullName(
            [FromBody] UpdateFullNameDto fullName,
            CancellationToken cancellationToken = default
            )
        {

            var updateResult = await _userService.UpdateFullNameAsync(fullName, GetUserId(), cancellationToken);

            return updateResult.ToActionResult();

        }

        [HttpPatch("username")]
        public async Task<ActionResult<string>> UpdateUserName(
            [FromBody] UpdateUserNameDto userName, 
            CancellationToken cancellationToken = default)
        {

            var updateResult = await _userService.UpdateUserNameAsync(userName, GetUserId(), cancellationToken);

            return updateResult.ToActionResult();
        }

        [HttpPatch("email")]
        public async Task<ActionResult<string>> UpdateEmail(
            [FromBody] UpdateEmailDto email,
            CancellationToken cancellationToken = default
            )
        {

            var updateResult = await _userService.UpdateEmailAsync(email, GetUserId(), cancellationToken);

            return updateResult.ToActionResult();

        }

        [HttpPatch("date-of-birth")]
        public async Task<ActionResult<DateTime>> UpdateDateOfBirth(
            [FromBody] UpdateDateOfBirthDto dateOfBirth,
            CancellationToken cancellationToken = default
            )
        {

            var updateResult = await _userService.UpdateDateOfBirthAsync(dateOfBirth, GetUserId(), cancellationToken);

            return updateResult.ToActionResult();

        }

        [HttpPatch("gender")]
        public async Task<ActionResult<Gender>> UpdateGender(
            [FromBody] UpdateGenderDto gender,
            CancellationToken cancellationToken = default
            )
        {

            var updateResult = await _userService.UpdateGenderAsync(gender, GetUserId(), cancellationToken);

            return updateResult.ToActionResult();

        }

        [HttpPatch("weight")]
        public async Task<ActionResult<double>> UpdateWeight(
            [FromBody] UpdateWeightDto weight,
            CancellationToken cancellationToken = default
            )
        {

            var updateResult = await _userService.UpdateWeightAsync(weight, GetUserId(), cancellationToken);

            return updateResult.ToActionResult();

        }

        [HttpPatch("height")]
        public async Task<ActionResult<double>> UpdateHeight(
            [FromBody] UpdateHeightDto height,
            CancellationToken cancellationToken = default
            )
        {

            var updateResult = await _userService.UpdateHeightAsync(height, GetUserId(), cancellationToken);

            return updateResult.ToActionResult();

        }
        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    }


}
