namespace SSW.Rewards.Mobile.Services
{
    public interface INotificationActionHandler
    {
        Task HandleNotificationActionAsync(string action);
    }

    public class NotificationActionHandler : INotificationActionHandler
    {
        public async Task HandleNotificationActionAsync(string action)
        {
            if (string.IsNullOrEmpty(action))
                return;

            if (action.StartsWith("post:", StringComparison.OrdinalIgnoreCase))
            {
                var postIdStr = action.Substring("post:".Length);
                if (int.TryParse(postIdStr, out var postId))
                {
                    var parameters = new Dictionary<string, object> { { "PostId", postId } };
                    await Shell.Current.GoToAsync("postdetail", parameters);
                }
            }
        }
    }
}
