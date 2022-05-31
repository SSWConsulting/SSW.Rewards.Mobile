using SSW.Rewards.Models;
using System.Collections.Generic;

namespace SSW.Rewards.ViewModels
{
    public class ProfileCarouselViewModel
    {
        public CarouselType Type { get; set; }

        public List<Achievement> Achievements { get; set; } = new List<Achievement>();

        public List<Activity> RecentActivity { get; set; } = new List<Activity>();

        public List<Notification> Notifications { get; set; } = new List<Notification>();
    }

    public enum CarouselType
    {
        Achievements,
        RecentActivity,
        Notifications
    }
}
