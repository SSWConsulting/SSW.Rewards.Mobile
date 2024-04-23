using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Shared.DTOs.Users;
using SSW.Rewards.Application.Achievements.Commands.ClaimSocialMediaAchievementForUser;
using SSW.Rewards.Application.Users.Commands.DeleteMyProfile;
using SSW.Rewards.Application.Users.Commands.RegisterUser;
using SSW.Rewards.Application.Users.Commands.UploadProfilePic;
using SSW.Rewards.Application.Users.Commands.UpsertUserSocialMediaId;
using SSW.Rewards.Application.Users.Queries.GetCurrentUser;
using SSW.Rewards.Application.Users.Queries.GetCurrentUserRoles;
using SSW.Rewards.Application.Users.Queries.GetNewUsers;
using SSW.Rewards.Application.Users.Queries.GetProfileAchievements;
using SSW.Rewards.Application.Users.Queries.GetUser;
using SSW.Rewards.Application.Users.Queries.GetUserAchievements;
using SSW.Rewards.Application.Users.Queries.GetUserRewards;
using SSW.Rewards.Enums;
using SSW.Rewards.WebAPI.Authorisation;
using SSW.Rewards.Application.Users.Commands.AdminDeleteProfile;
using SSW.Rewards.Application.Users.Queries.AdminGetProfileDeletionRequests;
using SSW.Rewards.Application.Users.Queries.GetUsers;

namespace SSW.Rewards.WebAPI.Controllers;

public class UserController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CurrentUserDto>> Get()
    {
        return Ok(await Mediator.Send(new GetCurrentUserQuery()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserProfileDto>> GetUser(int id)
    {
        return Ok(await Mediator.Send(new GetUserQuery() { Id = id }));
    }

    [HttpGet]
    public async Task<ActionResult<UserAchievementsViewModel>> Achievements([FromQuery] int userId)
    {
        return Ok(await Mediator.Send(new GetUserAchievementsQuery { UserId = userId }));
    }

    [HttpGet]
    public async Task<ActionResult<UserRewardsViewModel>> Rewards([FromQuery] int userId)
    {
        return Ok(await Mediator.Send(new GetUserRewardsQuery { UserId = userId }));
    }

    [HttpGet]
    public async Task<ActionResult<UserAchievementsViewModel>> ProfileAchievements([FromQuery] int userId)
    {
        return Ok(await Mediator.Send(new GetProfileAchivementsQuery { UserId = userId }));
    }

    [HttpPost]
    public async Task<ActionResult<ProfilePicResponseDto>> UploadProfilePic()
    {
        var file = HttpContext.Request.Form.Files["file"];
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file received");
        }

        return Ok(await Mediator.Send(new UploadProfilePicCommand { File = file.OpenReadStream() }));
    }

    [HttpGet]
    public async Task<ActionResult<string[]>> MyRoles()
    {
        return Ok(await Mediator.Send(new GetCurrentUserRolesQuery()));
    }

    [HttpPost]
    public async Task<ActionResult> Register()
    {
        await Mediator.Send(new RegisterUserCommand());
        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult<int>> UpsertUserSocialMediaId(UserSocialMediaIdDto dto)
    {
        // get the isInsert to save ourselves some db round-trips when possible.
        bool isInsert = await Mediator.Send(new UpsertUserSocialMediaIdCommand
        {
            AchievementId = dto.AchievementId, SocialMediaUserId = dto.SocialMediaUserId
        });

        int retVal = 0;
        if (isInsert)
        {
            // set achievement and return achievement id
            retVal = await Mediator.Send(new ClaimSocialMediaAchievementForUser { AchievementId = dto.AchievementId });
        }

        return Ok(retVal);
    }

    [HttpPost]
    public async Task<ActionResult> DeleteMyProfile()
    {
        await Mediator.Send(new DeleteMyProfileCommand());

        return Ok();
    }

    [HttpGet]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<UsersViewModel>> GetUsers()
    {
        return await Mediator.Send(new GetUsersQuery());
    }

    [HttpPost]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<IActionResult> UpdateUserRoles(UserDto dto)
    {
        await Mediator.Send(new AdminUpdateUserRolesCommand { User = dto });
        return Accepted();
    }

    [HttpGet]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<NewUsersViewModel>> GetNewUsers([FromQuery] LeaderboardFilter filter, bool filterStaff)
    {
        return await Mediator.Send(new GetNewUsersQuery { Filter = filter, FilterStaff = filterStaff });
    }

    [HttpPost]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<IActionResult> DeleteUserProfile(AdminDeleteProfileDto dto)
    {
        await Mediator.Send(new AdminDeleteProfileCommand { Profile = dto });
        return Accepted();
    }

    [HttpGet]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<ProfileDeletionRequestsVieWModel>> GetProfileDeletionRequests()
    {
        return await Mediator.Send(new AdminGetProfileDeletionRequestsQuery());
    }
}