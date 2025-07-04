using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Shared.DTOs.Achievements;
using SSW.Rewards.Application.Achievements.Command.ClaimAchievementForUser;
using SSW.Rewards.Application.Achievements.Command.ClaimFormCompletedAchievement;
using SSW.Rewards.Application.Achievements.Command.DeleteAchievement;
using SSW.Rewards.Application.Achievements.Command.PostAchievement;
using SSW.Rewards.Application.Achievements.Command.UpdateAchievement;
using SSW.Rewards.Application.Achievements.Queries.GetAchievementAdminList;
using SSW.Rewards.Application.Achievements.Queries.GetAchievementList;
using SSW.Rewards.Application.Achievements.Queries.GetAchievementUsers;
using SSW.Rewards.Application.Achievements.Queries.SearchAchievements;
using SSW.Rewards.WebAPI.Authorisation;

namespace SSW.Rewards.WebAPI.Controllers;

public class AchievementController : ApiControllerBase
{

    [HttpGet]
    public async Task<ActionResult<AchievementListViewModel>> List()
    {
        return Ok(await Mediator.Send(new GetAchievementListQuery()));
    }

    [HttpGet]
    public async Task<ActionResult<AchievementListViewModel>> Search([FromQuery] string searchTerm, [FromQuery] int page = 0, [FromQuery] int pageSize = 50)
    {
        return Ok(await Mediator.Send(new SearchAchievementQuery { SearchTerm = searchTerm, Page = page, PageSize = pageSize }));
    }

    [HttpGet]
    public async Task<ActionResult<AchievementUsersViewModel>> Users([FromQuery] int achievementId)
    {
        return Ok(await Mediator.Send(new GetAchievementUsersQuery { AchievementId = achievementId }));
    }

    [Authorize(Roles = AuthorizationRoles.Admin)]
    [HttpGet]
    public async Task<ActionResult<AchievementAdminListViewModel>> AdminList([FromQuery] bool includeArchived = false)
    {
        return Ok(await Mediator.Send(new GetAchievementAdminListQuery
        {
            IncludeArchived = includeArchived,
        }));
    }

    [HttpPost]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<AchievementAdminDto>> Create([FromBody] AchievementEditDto dto)
    {
        var command = new CreateAchievementCommand
        {
            Name = dto.Name,
            Value = dto.Value,
            Type = dto.Type,
            IsMultiscanEnabled = dto.IsMultiscanEnabled,
        };

        return Ok(await Mediator.Send(command));
    }

    [HttpPost]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<ClaimAchievementResult>> ClaimForUser([FromBody] ClaimAchievementForUserDto dto)
    {
        var command = new ClaimAchievementForUserCommand
        {
            UserId = dto.UserId,
            Code = dto.Code,
        };

        return Ok(await Mediator.Send(command));
    }


    [HttpPost]
    [Authorize(Policy = Policies.MobileApp)]
    public async Task<ActionResult<ClaimAchievementResult>> Claim([FromBody] string code)
    {
        return Ok(await Mediator.Send(new PostAchievementCommand { Code = code }));
    }

    [HttpDelete]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> Delete([FromQuery] int id)
    {
        await Mediator.Send(new DeleteAchievementCommand { Id = id });
        return Ok();
    }

    [HttpPatch]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> UpdateAchievement(AchievementEditDto dto)
    {
        var command = new UpdateAchievementCommand
        {
            Id = dto.Id,
            Value = dto.Value,
            Type = dto.Type,
            IsMultiscanEnabled = dto.IsMultiscanEnabled,
        };

        return Ok(await Mediator.Send(command));
    }

    [HttpPost]
    [AllowAnonymous]
    // TODO: Check whether we can make this authenticated
    public async Task<ActionResult> ClaimFormCompleted(ClaimFormCompletedAchievementCommand command)
    {
        await Mediator.Send(command);
        return Ok();
    }
}