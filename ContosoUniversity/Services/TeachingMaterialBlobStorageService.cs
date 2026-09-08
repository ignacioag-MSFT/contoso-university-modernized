using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ContosoUniversity.Services
{
    public class TeachingMaterialBlobStorageService
    {
        private const string DefaultContainerName = "teaching-materials";
        private readonly IConfiguration _configuration;
        private readonly Lazy<BlobContainerClient> _containerClient;

        public TeachingMaterialBlobStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            _containerClient = new Lazy<BlobContainerClient>(CreateContainerClient);
        }

        public async Task<string> UploadAsync(IFormFile teachingMaterialImage, string blobName, CancellationToken cancellationToken = default)
        {
            var containerClient = _containerClient.Value;
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlobClient(blobName);
            using var stream = teachingMaterialImage.OpenReadStream();
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = GetContentType(blobName)
                }
                // Conditions intentionally omitted -> unconditional overwrite.
            };

            // MIGRATION NOTE: unconditional overwrite preserves the previous local File.Create behavior.
            await blobClient.UploadAsync(stream, uploadOptions, cancellationToken);
            return blobName;
        }

        public async Task<DownloadedTeachingMaterial> DownloadAsync(string teachingMaterialImagePath, CancellationToken cancellationToken = default)
        {
            var blobName = GetBlobName(teachingMaterialImagePath);
            if (string.IsNullOrWhiteSpace(blobName))
            {
                return null;
            }

            var blobClient = _containerClient.Value.GetBlobClient(blobName);
            Azure.Response<BlobDownloadStreamingResult> downloadResponse;
            try
            {
                downloadResponse = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }

            using var download = downloadResponse.Value;
            var content = new MemoryStream();
            await download.Content.CopyToAsync(content, cancellationToken);
            content.Position = 0;

            return new DownloadedTeachingMaterial
            {
                Content = content,
                ContentType = GetContentType(blobName)
            };
        }

        public async Task DeleteIfExistsAsync(string teachingMaterialImagePath, CancellationToken cancellationToken = default)
        {
            var blobName = GetBlobName(teachingMaterialImagePath);
            if (string.IsNullOrWhiteSpace(blobName))
            {
                return;
            }

            var blobClient = _containerClient.Value.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }

        public string GetBlobName(string teachingMaterialImagePath)
        {
            if (string.IsNullOrWhiteSpace(teachingMaterialImagePath))
            {
                return null;
            }

            if (Uri.TryCreate(teachingMaterialImagePath, UriKind.RelativeOrAbsolute, out var uri) &&
                !string.IsNullOrWhiteSpace(uri.Query))
            {
                var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
                if (query.TryGetValue("blobName", out var blobNameFromQuery))
                {
                    return blobNameFromQuery.ToString();
                }
            }

            var normalizedPath = teachingMaterialImagePath
                .TrimStart('~', '/', '\\')
                .Replace('\\', '/');

            if (normalizedPath.Contains('/'))
            {
                return Path.GetFileName(normalizedPath);
            }

            return normalizedPath;
        }

        private BlobContainerClient CreateContainerClient()
        {
            var serviceUri = _configuration["Storage:ServiceUri"];
            if (string.IsNullOrWhiteSpace(serviceUri))
            {
                var storageAccountName = _configuration["Storage:StorageAccountName"];
                if (string.IsNullOrWhiteSpace(storageAccountName))
                {
                    throw new InvalidOperationException("Azure Blob Storage is not configured. Set Storage:ServiceUri or Storage:StorageAccountName.");
                }

                serviceUri = $"https://{storageAccountName}.blob.core.windows.net";
            }

            var containerName = _configuration["Storage:TeachingMaterialsContainerName"];
            if (string.IsNullOrWhiteSpace(containerName))
            {
                containerName = DefaultContainerName;
            }

            var blobServiceClient = new BlobServiceClient(new Uri(serviceUri), new DefaultAzureCredential());
            return blobServiceClient.GetBlobContainerClient(containerName);
        }

        private static string GetContentType(string blobName)
        {
            return Path.GetExtension(blobName).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }
    }

    public class DownloadedTeachingMaterial
    {
        public Stream Content { get; set; }
        public string ContentType { get; set; }
    }
}
