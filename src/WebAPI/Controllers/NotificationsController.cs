using System.ComponentModel.DataAnnotations;
using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Notifications.Commands.UploadDeviceToken;
using SSW.Rewards.Application.Notifications.Commands;
using SSW.Rewards.Application.Notifications.Commands.DeleteInstallation;
using SSW.Rewards.Application.Notifications.Commands.RequestNotification;
using SSW.Rewards.Application.Notifications.Commands.UpdateInstallation;
using SSW.Rewards.Application.Notifications.Queries;
using SSW.Rewards.Application.Notifications.Queries.GetNotificationHistoryList;
using SSW.Rewards.Shared.DTOs.Notifications;

namespace SSW.Rewards.WebAPI.Controllers;

//TODO: Pending V2 admin portal
public class NotificationsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<NotificationHistoryListViewModel>> List(int page = 0, int pageSize = 10, string? search = null, string? sortLabel = null, string? sortDirection = null, bool includeDeleted = false)
    {
        return Ok(await Mediator.Send(new GetNotificationHistoryListQuery { Page = page, PageSize = pageSize, Search = search, SortLabel = sortLabel, SortDirection = sortDirection, IncludeDeleted = includeDeleted }));
    }

    [HttpPost]
    public async Task<ActionResult<int>> ImpactedUsers(GetNumberOfImpactedNotificationUsersQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<Unit>> RequestPush([Required]RequestNotificationCommand notificationRequest)
    {
        return Ok(await Mediator.Send(notificationRequest));
    }

    [HttpPost]
    [ProducesResponseType(typeof(NotificationSentResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<NotificationSentResponse>> SendToUsersWithAchievements([FromBody][Required] SendNotificationToUsersWithAchievementsCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(NotificationSentResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<NotificationSentResponse>> SendToUser([FromBody][Required] SendNotificationToUserCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<Unit>> UploadDeviceToken([Required] DeviceTokenDto dto)
    {
        return Ok(await Mediator.Send(new UploadDeviceTokenCommand
        {
            DeviceToken = dto.DeviceToken,
            LastTimeUpdated = dto.LastTimeUpdated,
            DeviceId = dto.DeviceId,
        }));
    }

    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<Unit>> UpdateInstallation([Required] UpdateInstallationCommand deviceInstallation)
    {
        return Ok(await Mediator.Send(deviceInstallation));
    }

    [HttpDelete("{installationId}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<Unit>> DeleteInstallation([Required][FromRoute]string installationId)
    {
        return Ok(await Mediator.Send(new DeleteInstallationCommand(installationId)));
    }

    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<NotificationSentResponse>> SendAdminNotification(SendAdminNotificationCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> DeleteNotification([FromRoute] int id)
    {
        await Mediator.Send(new DeleteNotificationCommand(id));
        return Ok();
    }
}
