namespace SSW.Rewards.Mobile.ViewModels;

public class ScanResponseViewModel
{
    public int Points { get; set; }
    public string Title { get; set; }
    public ScanResult result { get; set; }
    public ScanType ScanType { get; set; }
    public int? ScannedUserId { get; set; }

    public static ScanResponseViewModel NotFound()
        => new() { result = ScanResult.NotFound, Title = "Not Found" };

    public static ScanResponseViewModel OnlyStaffCanRedeemPendingRewards()
        => new() { result = ScanResult.Error, Title = "Only SSW can redeem pending rewards" };
}
