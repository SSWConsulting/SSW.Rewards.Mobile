using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Users.Queries.GetCurrentUser;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.WebAPI.Security
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RestrictedAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cus = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

            CurrentUserViewModel u = await cus.GetCurrentUserAsync(CancellationToken.None);

            if (u == null)
            {
                context.Result = new UnauthorizedResult();
            }
            else
            {
                await base.OnActionExecutionAsync(context, next);
            }
        }
    }
}
