using QRCoder;

namespace SSW.Rewards.Mobile.Helpers;

public static class ImageHelpers
{
    public static ImageSource GenerateQrCode(string qrCodeString)
    {
        using QRCodeGenerator qrGenerator = new();
        using QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeString, QRCodeGenerator.ECCLevel.Q);
        using PngByteQRCode qrCode = new(qrCodeData);
        
        byte[] qrCodeBytes = qrCode.GetGraphic(20);
        return ImageSource.FromStream(() => new MemoryStream(qrCodeBytes));
    }
}
