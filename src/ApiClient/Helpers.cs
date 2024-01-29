using System.Net.Http.Headers;

namespace SSW.Rewards.ApiClient;

public static class Helpers
{
    public static MultipartFormDataContent ProcessImageContent(Stream file, string fileName)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(file);

        var mimeType = GetMimeType(fileName);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

        content.Add(fileContent, "file", fileName);
        return content;
    }
    
    private static string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            // Add other extensions and MIME types as needed
            _ => "application/octet-stream", // default MIME type
        };
    }
}