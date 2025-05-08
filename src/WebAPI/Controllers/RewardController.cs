﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Shared.DTOs.Rewards;
using SSW.Rewards.Application.Rewards.Commands;
using SSW.Rewards.Application.Rewards.Commands.AddReward;
using SSW.Rewards.Application.Rewards.Commands.ClaimReward;
using SSW.Rewards.Application.Rewards.Commands.DeleteReward;
using SSW.Rewards.Application.Rewards.Commands.UpdateReward;
using SSW.Rewards.Application.Rewards.Queries.GetOnboardingRewards;
using SSW.Rewards.Application.Rewards.Queries.GetRecentRewards;
using SSW.Rewards.Application.Rewards.Queries.GetRewardList;
using SSW.Rewards.Application.Rewards.Queries.SearchRewards;
using SSW.Rewards.WebAPI.Authorisation;
using SSW.Rewards.Application.Rewards.Queries.GetRewardAdminList;

namespace SSW.Rewards.WebAPI.Controllers;

public class RewardController : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<RewardListViewModel>> GetOnboardingRewards()
    {
        return Ok(await Mediator.Send(new GetOnboardingRewards()));
    }

    [HttpGet]
    public async Task<ActionResult<RewardListViewModel>> List()
    {
        return Ok(await Mediator.Send(new GetRewardList()));
    }

    [HttpGet]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<RewardsAdminViewModel>> AdminList()
    {
        return Ok(await Mediator.Send(new GetRewardAdminListQuery()));
    }

    [HttpGet]
    public async Task<ActionResult<RewardListViewModel>> Search([FromQuery] string searchTerm, [FromQuery] int page = 0, [FromQuery] int pageSize = 50)
    {
        return Ok(await Mediator.Send(new SearchRewardsQuery { SearchTerm = searchTerm, Page = page, PageSize = pageSize }));
    }

    [HttpGet]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<RecentRewardListViewModel>> GetRecent(GetRecentRewardsQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpPost]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<int>> Add(RewardEditDto dto)
    {
        var command = new AddRewardCommand
        {
            Name = dto.Name,
            Description = dto.Description,
            Cost = dto.Cost,
            IsHidden = dto.IsHidden,
            RewardType = dto.RewardType,
            ImageBytesInBase64 = dto.ImageBytesInBase64,
            ImageFileName = dto.ImageFileName,
            CarouselImageBytesInBase64 = dto.CarouselImageBytesInBase64,
            CarouselImageFileName = dto.CarouselImageFileName,
            IsCarousel = dto.IsCarousel
        };
        
        return Ok(await Mediator.Send(command));
    }

    [HttpPost]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<ClaimRewardResult>> ClaimForUser(ClaimRewardForUserCommand claimRewardForUserCommand)
    {
        return Ok(await Mediator.Send(claimRewardForUserCommand));
    }

    [HttpPost]
    [Authorize(Policy = Policies.MobileApp)]
    public async Task<ActionResult<ClaimRewardResult>> Claim(ClaimRewardDto claim)
    {
        return Ok(await Mediator.Send(new ClaimRewardCommand
        {
            Code = claim.Code,
            Id = claim.Id,
            Address = claim.Address,
            ClaimInPerson = claim.InPerson
        }));
    }
    
    [HttpPost]
    [Authorize(Policy = Policies.MobileApp)]
    public async Task<ActionResult<CreatePendingRedemptionResult>> CreatePendingRedemption(CreatePendingRedemptionDto claim)
    {
        return Ok(await Mediator.Send(new CreatePendingRedemptionCommand()
        {
            Id = claim.Id,
        }));
    }
    
    [HttpPost]
    [Authorize(Policy = Policies.MobileApp)]
    public async Task<ActionResult<CancelPendingRedemptionResult>> CancelPendingRedemption(CancelPendingRedemptionDto claim)
    {
        return Ok(await Mediator.Send(new CancelPendingRedemptionCommand()
        {
            Id = claim.Id,
        }));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> Delete(int id)
    {
        var command = new DeleteRewardCommand { Id = id };
        return Ok(await Mediator.Send(command));
    }

    [HttpPatch]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> UpdateReward(RewardEditDto dto)
    {
        var command = new UpdateRewardCommand
        {
            Id = dto.Id,
            RewardName = dto.Name,
            Description = dto.Description,
            Cost = dto.Cost,
            IsHidden = dto.IsHidden,
            ImageBytesInBase64 = dto.ImageBytesInBase64,
            ImageFilename = dto.ImageFileName,
            IsOnboardingReward = dto.IsOnboardingReward,
            CarouselImageBytesInBase64 = dto.CarouselImageBytesInBase64,
            CarouselImageFileName = dto.CarouselImageFileName,
            IsCarousel = dto.IsCarousel,
            DeleteThumbnailImage = dto.DeleteThumbnailImage,
            DeleteCarouselImage = dto.DeleteCarouselImage,
            RewardType = dto.RewardType
        };

        return Ok(await Mediator.Send(command));
    }
}
