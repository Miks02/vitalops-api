using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using VitalOps.API.DTO.User;
using VitalOps.API.Enums;
using VitalOps.API.Services.Interfaces;
using VitalOps.API.DTO.Global;
using VitalOps.API.Extensions;

namespace VitalOps.API.Controllers
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDetailsDto>> GetMe()
        {
            return await _userService.GetUserDetailsAsync(CurrentUserId);
        }

        [HttpPatch("fullname")]
        public async Task<ActionResult<string>> UpdateFullName(
            [FromBody] UpdateFullNameDto fullName,
            CancellationToken cancellationToken = default
        )
        {

            var updateResult = await _userService.UpdateFullNameAsync(fullName, CurrentUserId, cancellationToken);

            return updateResult.ToActionResult();

        }

        [HttpPatch("username")]
        public async Task<ActionResult<string>> UpdateUserName(
            [FromBody] UpdateUserNameDto userName,
            CancellationToken cancellationToken = default)
        {

            var updateResult = await _userService.UpdateUserNameAsync(userName, CurrentUserId, cancellationToken);

            return updateResult.ToActionResult();
        }

        [HttpPatch("email")]
        public async Task<ActionResult<string>> UpdateEmail(
            [FromBody] UpdateEmailDto email,
            CancellationToken cancellationToken = default
        )
        {

            var updateResult = await _userService.UpdateEmailAsync(email, CurrentUserId, cancellationToken);

            return updateResult.ToActionResult();

        }

        [HttpPatch("date-of-birth")]
        public async Task<ActionResult<DateTime>> UpdateDateOfBirth(
            [FromBody] UpdateDateOfBirthDto dateOfBirth,
            CancellationToken cancellationToken = default
        )
        {

            var updateResult = await _userService.UpdateDateOfBirthAsync(dateOfBirth, CurrentUserId, cancellationToken);

            return updateResult.ToActionResult();

        }

        [HttpPatch("gender")]
        public async Task<ActionResult<Gender>> UpdateGender(
            [FromBody] UpdateGenderDto gender,
            CancellationToken cancellationToken = default
        )
        {

            var updateResult = await _userService.UpdateGenderAsync(gender, CurrentUserId, cancellationToken);

            return updateResult.ToActionResult();

        }

        [HttpPatch("height")]
        public async Task<ActionResult<double>> UpdateHeight(
            [FromBody] UpdateHeightDto height,
            CancellationToken cancellationToken = default
        )
        {

            var updateResult = await _userService.UpdateHeightAsync(height, CurrentUserId, cancellationToken);

            return updateResult.ToActionResult();

        }

        [HttpPatch("target-weight")]
        public async Task<ActionResult<double>> UpdateTargetWeight(
            [FromBody] UpdateTargetWeightDto targetWeight,
            CancellationToken cancellationToken = default
        )
        {

            var updateResult =
                await _userService.UpdateTargetWeightAsync(targetWeight, CurrentUserId, cancellationToken);

            return updateResult.ToActionResult();

        }

    }


}
