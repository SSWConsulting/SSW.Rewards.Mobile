using CommunityToolkit.Mvvm.ComponentModel;

namespace SSW.Rewards.Models
{
    public partial class Reward : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Cost { get; set; }
        public bool IsDigital { get; set; }
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

        public static bool IsEqual(Reward? r1, Reward? r2)
        {
            if (r1 is null && r2 is null) return true;
            if (r1 is null || r2 is null) return false;
            return r1.Id == r2.Id &&
                   r1.Name == r2.Name &&
                   r1.Description == r2.Description &&
                   r1.Cost == r2.Cost &&
                   r1.IsDigital == r2.IsDigital &&
                   r1.AwardedAt == r2.AwardedAt &&
                   r1.Awarded == r2.Awarded &&
                   r1.ImageUri == r2.ImageUri &&
                   r1.CarouselImageUri == r2.CarouselImageUri &&
                   r1.IsCarousel == r2.IsCarousel &&
                   r1.IsHidden == r2.IsHidden &&
                   r1.IsPendingRedemption == r2.IsPendingRedemption &&
                   r1.PendingRedemptionCode == r2.PendingRedemptionCode;
        }
    }
}
