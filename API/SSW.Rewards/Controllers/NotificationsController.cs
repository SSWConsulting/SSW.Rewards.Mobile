using System.Threading.Tasks;
using System.Net;
using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Notifications.Commands.DeleteInstallation;
using SSW.Rewards.Application.Notifications.Commands.RequestNotification;
using SSW.Rewards.Application.Notifications.Commands.UpdateInstallation;
using SSW.Rewards.Application.Notifications.Queries.GetNotificationHistoryList;

namespace SSW.Rewards.WebAPI.Controllers
{
    public class NotificationsController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<NotificationHistoryListViewModel>> List()
        {
            return Ok(await Mediator.Send(new GetNotificationHistoryListQuery()));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<Unit>> RequestPush([Required] RequestNotificationCommand notificationRequest)
        {
            return Ok(await Mediator.Send(notificationRequest));
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
        public async Task<ActionResult<Unit>> DeleteInstallation([Required][FromRoute] DeleteInstallationCommand installationId)
        {
            return Ok(await Mediator.Send(installationId));
        }
    }
}