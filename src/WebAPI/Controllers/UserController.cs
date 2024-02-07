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
    public async Task<ActionResult<ProfilePicResponseDto>> UploadProfilePic(Stream file)
    {
        return Ok(await Mediator.Send(new UploadProfilePicCommand { File = file }));
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
    public async Task<ActionResult<int>> UpsertUserSocialMediaId(UpsertUserSocialMediaId command)
    {
        int retVal = 0;
        // get the isInsert to save ourselves some db round-trips when possible.
        bool isInsert = await Mediator.Send(command);
        if (isInsert)
        {
            // set achievement and return achievement id
            retVal = await Mediator.Send(new ClaimSocialMediaAchievementForUser { AchievementId = command.AchievementId });
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
    public async Task<ActionResult<NewUsersViewModel>> FindNewUsers([FromQuery] LeaderboardFilter filter, bool filterStaff)
    {
        return await Mediator.Send(new GetNewUsersQuery { Filter = filter, FilterStaff = filterStaff });
    }
}