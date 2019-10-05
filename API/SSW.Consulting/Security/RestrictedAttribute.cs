using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SSW.Consulting.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.WebAPI.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RestrictedAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cus = context.HttpContext.RequestServices.GetService<ICurrentUserService>();

            var u = await cus.GetCurrentUser(CancellationToken.None);
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
