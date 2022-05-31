using System;
using System.Collections.Generic;
using System.Linq;

using SSW.Rewards.Models;

namespace SSW.Rewards.Services
{
    public class PushNotificationActionService : IPushNotificationActionService
    {
        /// <summary>
        /// TODO: Specify the actions of which determines which message user/s will receive
        /// </summary>
        readonly Dictionary<string, PushNotificationAction> _actionMappings = new Dictionary<string, PushNotificationAction>
        {
            { "action_a", PushNotificationAction.ActionA },
            { "action_b", PushNotificationAction.ActionB }
        };

        public event EventHandler<PushNotificationAction> ActionTriggered = delegate { };

        public void TriggerAction(string action)
        {
            if (!_actionMappings.TryGetValue(action, out var pushAction))
                return;

            List<Exception> exceptions = new List<Exception>();

            foreach (var handler in ActionTriggered?.GetInvocationList())
            {
                try
                {
                    handler.DynamicInvoke(this, pushAction);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }
    }
}