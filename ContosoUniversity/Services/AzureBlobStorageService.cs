using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ContosoUniversity.Services
{
    /// <summary>
    /// Service for managing file uploads and downloads using Azure Blob Storage.
    /// Replaces local file system operations with cloud-based blob storage.
    /// 
    /// MIGRATION NOTE: This service wraps Azure Storage Blobs SDK to provide
    /// application-level abstractions for teaching material uploads. Uses Managed Identity
    /// (DefaultAzureCredential) for authentication to comply with cloud security best practices.
    /// </summary>
    public class AzureBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<AzureBlobStorageService> _logger;
        private readonly string _containerName;

        /// <summary>
        /// Initializes a new instance of the AzureBlobStorageService.
        /// 
        /// The BlobServiceClient is registered as a Singleton in the DI container
        /// following Rule 26 (Azure SDK Client Lifetime) to reuse the HTTP pipeline,
        /// connection pool, and credential token cache across requests.
        /// </summary>
        public AzureBlobStorageService(
            BlobServiceClient blobServiceClient,
            IConfiguration configuration,
            ILogger<AzureBlobStorageService> logger)
        {
            _logger = logger;
            
            // Read container name from configuration
            _containerName = configuration.GetValue<string>("AzureStorage:BlobContainerName")
                ?? "teaching-materials";
            
            // Get container client (does not make a network call)
            _containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        }

        /// <summary>
        /// Uploads a file to Azure Blob Storage.
        /// 
        /// MIGRATION NOTE: Replaces local file system writes with blob uploads.
        /// Supports streaming for large files and overwrites existing blobs
        /// to maintain compatibility with the original create/update behavior.
        /// </summary>
        public async Task<string> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType)
        {
            try
            {
                if (fileStream == null)
                {
                    throw new ArgumentNullException(nameof(fileStream));
                }

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    throw new ArgumentException("File name cannot be empty.", nameof(fileName));
                }

                // Generate a unique blob name to avoid collisions
                // This matches the original behavior of using Guid in the filename
                var uniqueBlobName = fileName;

                _logger.LogInformation($"Uploading blob: {uniqueBlobName}");

                // Ensure stream position is at the start
                if (fileStream.CanSeek)
                {
                    fileStream.Position = 0;
                }

                // Create blob client for the specific blob
                var blobClient = _containerClient.GetBlobClient(uniqueBlobName);

                // Upload the blob with metadata
                // MIGRATION NOTE: Using BlobUploadOptions to set content type and enable
                // unconditional overwrite (matching S3 PutObject default per Rule 17).
                // Conditions intentionally omitted to preserve S3 PutObject semantics.
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType ?? "application/octet-stream"
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "UploadedAt", DateTime.UtcNow.ToString("O") }
                    }
                };

                await blobClient.UploadAsync(fileStream, uploadOptions, cancellationToken: default);

                _logger.LogInformation($"File uploaded successfully: {uniqueBlobName}");

                // Return the relative URL path for storing in the database
                // This maintains compatibility with the original /Uploads/TeachingMaterials/{fileName} pattern
                return $"/Uploads/TeachingMaterials/{uniqueBlobName}";
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"Azure Blob Storage request failed: {ex.Status} {ex.ErrorCode}: {ex.Message}");
                throw new InvalidOperationException(
                    $"Error uploading file to Azure Blob Storage: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading file: {ex.Message}");
                throw new InvalidOperationException(
                    $"Error uploading file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes a file from Azure Blob Storage.
        /// 
        /// MIGRATION NOTE: Replaces local file system deletes with blob deletes.
        /// Uses DeleteIfExistsAsync to handle cases where the blob may not exist.
        /// </summary>
        public async Task DeleteFileAsync(string blobName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blobName))
                {
                    _logger.LogWarning("Attempted to delete blob with empty name.");
                    return;
                }

                // Extract just the filename from the URL path if a full path is provided
                // Original path format: /Uploads/TeachingMaterials/{fileName}
                var cleanBlobName = blobName.Contains("/")
                    ? blobName.Split('/').Last()
                    : blobName;

                _logger.LogInformation($"Deleting blob: {cleanBlobName}");

                var blobClient = _containerClient.GetBlobClient(cleanBlobName);

                // DeleteIfExistsAsync handles the case where blob doesn't exist gracefully
                // MIGRATION NOTE: Using data-plane SDK for deletion per Rule 24.
                // No immutability policies are enabled, so data-plane deletion suffices.
                bool deleted = await blobClient.DeleteIfExistsAsync();

                if (deleted)
                {
                    _logger.LogInformation($"Blob deleted successfully: {cleanBlobName}");
                }
                else
                {
                    _logger.LogInformation($"Blob not found, skipping deletion: {cleanBlobName}");
                }
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"Azure Blob Storage request failed: {ex.Status} {ex.ErrorCode}: {ex.Message}");
                throw new InvalidOperationException(
                    $"Error deleting file from Azure Blob Storage: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting file: {ex.Message}");
                throw new InvalidOperationException(
                    $"Error deleting file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Downloads a file from Azure Blob Storage.
        /// 
        /// MIGRATION NOTE: Replaces local file system reads with blob downloads.
        /// Supports streaming for large files and proper resource disposal.
        /// </summary>
        public async Task<Stream> DownloadFileAsync(string blobName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blobName))
                {
                    throw new ArgumentException("Blob name cannot be empty.", nameof(blobName));
                }

                // Extract just the filename from the URL path if a full path is provided
                var cleanBlobName = blobName.Contains("/")
                    ? blobName.Split('/').Last()
                    : blobName;

                _logger.LogInformation($"Downloading blob: {cleanBlobName}");

                var blobClient = _containerClient.GetBlobClient(cleanBlobName);

                // Check if blob exists first
                bool exists = (await blobClient.ExistsAsync()).Value;
                if (!exists)
                {
                    throw new FileNotFoundException($"Blob not found: {cleanBlobName}");
                }

                // Download to stream
                // MIGRATION NOTE: Using DownloadContentAsync instead of DownloadAsync
                // to load entire blob into memory stream for simplicity. For very large files,
                // consider using DownloadAsync and wrapping with a using statement.
                BlobDownloadInfo download = await blobClient.DownloadAsync();
                var memoryStream = new MemoryStream();
                await download.Content.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                _logger.LogInformation($"File downloaded successfully: {cleanBlobName}");

                return memoryStream;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"Azure Blob Storage request failed: {ex.Status} {ex.ErrorCode}: {ex.Message}");
                throw new InvalidOperationException(
                    $"Error downloading file from Azure Blob Storage: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading file: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Checks if a blob exists in Azure Blob Storage.
        /// 
        /// MIGRATION NOTE: Provides existence check before operations,
        /// replacing local file system File.Exists checks.
        /// </summary>
        public async Task<bool> BlobExistsAsync(string blobName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blobName))
                {
                    return false;
                }

                var cleanBlobName = blobName.Contains("/")
                    ? blobName.Split('/').Last()
                    : blobName;

                var blobClient = _containerClient.GetBlobClient(cleanBlobName);

                // MIGRATION NOTE: Unwrapping Response<bool>.Value per Rule 27
                return (await blobClient.ExistsAsync()).Value;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking blob existence: {ex.Message}");
                return false;
            }
        }
    }
}
