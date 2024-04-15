using System.ComponentModel.DataAnnotations;
using System.Net;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Notifications.Commands.UploadDeviceToken;
using SSW.Rewards.Application.Notifications.Commands.DeleteInstallation;
using SSW.Rewards.Application.Notifications.Commands.RequestNotification;
using SSW.Rewards.Application.Notifications.Commands.UpdateInstallation;
using SSW.Rewards.Application.Notifications.Queries.GetNotificationHistoryList;

namespace SSW.Rewards.WebAPI.Controllers;

//TODO: Pending V2 admin portal
[AllowAnonymous]
public class NotificationsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<NotificationHistoryListViewModel>> List()
    {
        return Ok(await Mediator.Send(new GetNotificationHistoryListQuery()));
    }

    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<Unit>> RequestPush([Required]RequestNotificationCommand notificationRequest)
    {
        return Ok(await Mediator.Send(notificationRequest));
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
            DeviceId = dto.DeviceToken,
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
}
