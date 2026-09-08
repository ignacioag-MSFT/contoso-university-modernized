using System.Text.Json;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using ContosoUniversity.Models;
using Microsoft.Extensions.Configuration;

namespace ContosoUniversity.Services
{
    public class NotificationService : IDisposable
    {
        private const string AzureServiceBusSectionName = "AzureServiceBus";
        private const string DefaultNotificationQueueName = "contoso-university-notifications";
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _notificationQueueName;
        private bool _disposed;

        public NotificationService()
            : this(CreateConfiguration())
        {
        }

        public NotificationService(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _notificationQueueName = configuration[$"{AzureServiceBusSectionName}:NotificationQueueName"]
                ?? DefaultNotificationQueueName;

            var fullyQualifiedNamespace = configuration[$"{AzureServiceBusSectionName}:FullyQualifiedNamespace"];
            if (!string.IsNullOrWhiteSpace(fullyQualifiedNamespace))
            {
                _serviceBusClient = new ServiceBusClient(
                    fullyQualifiedNamespace,
                    new DefaultAzureCredential());
            }
        }

        public void SendNotification(string entityType, string entityId, EntityOperation operation, string userName = null)
        {
            SendNotification(entityType, entityId, null, operation, userName);
        }

        public void SendNotification(string entityType, string entityId, string entityDisplayName, EntityOperation operation, string userName = null)
        {
            try
            {
                var notification = new Notification
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Operation = operation.ToString(),
                    Message = GenerateMessage(entityType, entityId, entityDisplayName, operation),
                    CreatedAt = DateTime.Now,
                    CreatedBy = userName ?? "System",
                    IsRead = false
                };

                var jsonContent = JsonSerializer.Serialize(notification);
                if (!EnsureServiceBusConfigured("send"))
                    return;

                var sender = _serviceBusClient.CreateSender(_notificationQueueName);
                try
                {
                    sender.SendMessageAsync(new ServiceBusMessage(jsonContent)
                    {
                        ContentType = "application/json"
                    }).GetAwaiter().GetResult();
                }
                finally
                {
                    sender.DisposeAsync().AsTask().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to send notification: {ex.Message}");
            }
        }

        public Notification ReceiveNotification()
        {
            try
            {
                if (!EnsureServiceBusConfigured("receive"))
                    return null;

                var receiverOptions = new ServiceBusReceiverOptions
                {
                    ReceiveMode = ServiceBusReceiveMode.PeekLock
                };

                var receiver = _serviceBusClient.CreateReceiver(_notificationQueueName, receiverOptions);
                try
                {
                    var receivedMessage = receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(1)).GetAwaiter().GetResult();
                    if (receivedMessage == null)
                    {
                        return null;
                    }

                    try
                    {
                        var notification = JsonSerializer.Deserialize<Notification>(receivedMessage.Body.ToString());
                        receiver.CompleteMessageAsync(receivedMessage).GetAwaiter().GetResult();
                        return notification;
                    }
                    catch
                    {
                        receiver.AbandonMessageAsync(receivedMessage).GetAwaiter().GetResult();
                        throw;
                    }
                }
                finally
                {
                    receiver.DisposeAsync().AsTask().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to receive notification: {ex.Message}");
                return null;
            }
        }

        public void MarkAsRead(int notificationId)
        {
        }

        private string GenerateMessage(string entityType, string entityId, string entityDisplayName, EntityOperation operation)
        {
            var displayText = !string.IsNullOrWhiteSpace(entityDisplayName)
                ? $"{entityType} '{entityDisplayName}'"
                : $"{entityType} (ID: {entityId})";

            return operation switch
            {
                EntityOperation.CREATE => $"New {displayText} has been created",
                EntityOperation.UPDATE => $"{displayText} has been updated",
                EntityOperation.DELETE => $"{displayText} has been deleted",
                _ => $"{displayText} operation: {operation}"
            };
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _serviceBusClient?.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private bool EnsureServiceBusConfigured(string operation)
        {
            if (_serviceBusClient != null)
            {
                return true;
            }

            System.Diagnostics.Debug.WriteLine(
                $"Azure Service Bus namespace is not configured; unable to {operation} notification message.");
            return false;
        }
    }
}
