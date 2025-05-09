using CommunityToolkit.Mvvm.ComponentModel;

namespace SSW.Rewards.Models
{
    public partial class Reward : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Cost { get; set; }
        public DateTimeOffset? AwardedAt { get; set; }
        public bool Awarded { get; set; }
        public string ImageUri { get; set; }
        public string CarouselImageUri { get; set; }
        public bool IsCarousel { get; set; }
        public bool IsHidden { get; set; }
        public bool IsPendingRedemption { get; set; }
        public string PendingRedemptionCode { get; set; }
        
                
        [ObservableProperty]
        private bool _canAfford;
    }
}
