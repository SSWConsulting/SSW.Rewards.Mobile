using Microsoft.AspNetCore.Components.Forms;

namespace SSW.Rewards.Admin.UI.Helpers;

public class PhotoUploadHelper
{
    public static async Task<(string base64String, string fileName)> UploadPhoto(InputFileChangeEventArgs e)
    {
        var imageFile = e.File;

        var ms = new MemoryStream();

        await imageFile.OpenReadStream().CopyToAsync(ms);

        var bytes = ms.ToArray();

        return (Convert.ToBase64String(bytes), imageFile.Name);
    }

    public static async Task<(string base64String, string fileName)> UploadPhoto(IBrowserFile imageFile)
    {
        var ms = new MemoryStream();

        await imageFile.OpenReadStream().CopyToAsync(ms);

        var bytes = ms.ToArray();

        return (Convert.ToBase64String(bytes), imageFile.Name);
    }
}