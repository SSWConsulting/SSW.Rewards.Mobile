using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using SSW.Rewards.Application.Common.Models;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.WebAPI.Controllers
{
    public class NotificationsController : BaseController
    {
        readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.UnprocessableEntity)]
        public async Task<IActionResult> UpdateInstallation(
            [Required]DeviceInstall deviceInstallation)
        {
            var success = await _notificationService
                .CreateOrUpdateInstallationAsync(deviceInstallation, HttpContext.RequestAborted);

            if (!success)
                return new UnprocessableEntityResult();

            return new OkResult();
        }

        [HttpDelete("{installationId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.UnprocessableEntity)]
        public async Task<ActionResult> DeleteInstallation(
            [Required][FromRoute]string installationId)
        {
            var success = await _notificationService
                .DeleteInstallationByIdAsync(installationId, CancellationToken.None);

            if (!success)
                return new UnprocessableEntityResult();

            return new OkResult();
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.UnprocessableEntity)]
        public async Task<IActionResult> RequestPush(
            [Required]NotificationRequest notificationRequest)
        {
            if ((notificationRequest.Silent &&
                string.IsNullOrWhiteSpace(notificationRequest?.Action)) ||
                (!notificationRequest.Silent &&
                string.IsNullOrWhiteSpace(notificationRequest?.Text)))
                return new BadRequestResult();

            var success = await _notificationService
                .RequestNotificationAsync(notificationRequest, HttpContext.RequestAborted);

            if (!success)
                return new UnprocessableEntityResult();

            return new OkResult();
        }
    }
}
