using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.DataSeeder;

/// <summary>
/// Serves demo images (avatars, reward pictures) from the tool's bundled Assets folder,
/// uploading each to the local blob emulator on first use. Idempotent: a fixed blob name
/// per key means re-runs reuse the already-uploaded image. Returns null (and seeds on
/// without the image) when the asset or the emulator is unavailable.
/// </summary>
public class AzuriteAssetProvider(BlobServiceClient client, string assetsDirectory) : IDemoAssetProvider
{
    private const string ContainerName = "demo-assets";

    public async Task<string?> GetAssetUriAsync(string key, CancellationToken cancellationToken)
    {
        var file = FindAsset(key);
        if (file is null) return null;

        try
        {
            var container = client.GetBlobContainerClient(ContainerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
            var blob = container.GetBlobClient($"{key}{Path.GetExtension(file)}");
            if (!await blob.ExistsAsync(cancellationToken))
            {
                await using var stream = File.OpenRead(file);
                await blob.UploadAsync(stream, cancellationToken);
            }
            return blob.Uri.AbsoluteUri;
        }
        catch (Exception e)
        {
            Console.WriteLine($"  ! could not upload asset '{key}': {e.Message} — seeding without it");
            return null;
        }
    }

    private string? FindAsset(string key) =>
        new[] { "avatars", "rewards", "." }
            .SelectMany(dir => new[] { "png", "webp", "jpg" }
                .Select(ext => Path.Combine(assetsDirectory, dir, $"{key}.{ext}")))
            .FirstOrDefault(File.Exists);
}
