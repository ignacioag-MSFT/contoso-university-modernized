using System;
using Azure.Messaging.ServiceBus;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Services
{
    public class NotificationService : IAsyncDisposable
    {
        private readonly SchoolContext _context;
        private readonly ServiceBusSender? _sender;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            SchoolContext context,
            IConfiguration configuration,
            ILogger<NotificationService> logger,
            ServiceBusClient? serviceBusClient = null)
        {
            _context = context;
            _logger = logger;

            if (serviceBusClient != null)
            {
                var queueName = configuration["AzureServiceBus:QueueName"];
                if (string.IsNullOrWhiteSpace(queueName))
                {
                    throw new InvalidOperationException("AzureServiceBus:QueueName must be configured.");
                }

                _sender = serviceBusClient.CreateSender(queueName);
            }
        }

        public void SendNotification(string entityType, string entityId, EntityOperation operation, string? userName = null)
        {
            SendNotification(entityType, entityId, null, operation, userName);
        }

        public void SendNotification(string entityType, string entityId, string? entityDisplayName, EntityOperation operation, string? userName = null)
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

                _context.Notifications.Add(notification);
                _context.SaveChanges();

                if (_sender != null)
                {
                    var message = new ServiceBusMessage(BinaryData.FromObjectAsJson(notification))
                    {
                        ContentType = "application/json",
                        MessageId = notification.Id.ToString(),
                        Subject = $"{notification.EntityType}.{notification.Operation}"
                    };

                    _sender.SendMessageAsync(message).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification for {EntityType} {EntityId}", entityType, entityId);
            }
        }

        public Notification? ReceiveNotification()
        {
            try
            {
                return _context.Notifications
                    .AsNoTracking()
                    .OrderByDescending(n => n.CreatedAt)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to receive notification: {ex.Message}");
                return null;
            }
        }

        public void MarkAsRead(int notificationId)
        {
            try
            {
                var notification = _context.Notifications.Find(notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to mark notification as read: {ex.Message}");
            }
        }

        private string GenerateMessage(string entityType, string entityId, string? entityDisplayName, EntityOperation operation)
        {
            var displayText = !string.IsNullOrWhiteSpace(entityDisplayName) 
                ? $"{entityType} '{entityDisplayName}'" 
                : $"{entityType} (ID: {entityId})";

            return operation switch
            {
                EntityOperation.CREATE => $"New {displayText} has been created",
                EntityOperation.UPDATE => $"{displayText} has been updated",
                EntityOperation.DELETE => $"{displayText} has been deleted",
                _ => $"{displayText} operation: {operation}",
            };
        }

        public async ValueTask DisposeAsync()
        {
            if (_sender != null)
            {
                await _sender.DisposeAsync();
            }
        }
    }
}

