namespace SSW.Rewards.ViewModels
{
    public class ScanResponseViewModel
    {
        public int Points { get; set; }
        public string Title { get; set; }
        public ScanResult result { get; set; }
        public ScanType ScanType { get; set; }
    }
}
