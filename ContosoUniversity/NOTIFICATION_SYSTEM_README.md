# Real-Time Admin Notification System

This project includes a real-time notification system that alerts administrators whenever entity operations (create, update, delete) are performed in the system.

## Overview

The notification system uses **Azure Service Bus** as the messaging technology for reliable notifications to administrators. The application authenticates to Service Bus with `DefaultAzureCredential`, which supports managed identity in Azure-hosted environments.

## Features

- **Real-time notifications**: Admins receive immediate notifications when entities are modified
- **Entity coverage**: Monitors Students, Courses, Instructors, and Departments
- **Operation tracking**: Tracks CREATE, UPDATE, and DELETE operations
- **Admin-only**: Only users with administrator role receive notifications
- **Non-intrusive UI**: Notifications appear in the top-right corner with auto-dismiss
- **Reliable delivery**: Uses Azure Service Bus peek-lock receive and explicit message completion

## How It Works

### Backend Components

1. **NotificationService**: Sends and receives JSON notification messages through Azure Service Bus
2. **BaseController**: Base class that all controllers inherit from to send notifications
3. **Notification Model**: Entity to represent notification data
4. **NotificationsController**: API endpoints for retrieving notifications

### Frontend Components

1. **notifications.css**: Styling for notification UI elements
2. **notifications.js**: JavaScript polling system that checks for new notifications
3. **Layout integration**: Admin-only inclusion of notification assets

### Technology Stack

- **Azure Service Bus**: Managed cloud messaging service
- **Azure Identity**: Managed identity authentication
- **Entity Framework**: Data access for notification persistence
- **ASP.NET Core MVC**: Web framework
- **JavaScript/jQuery**: Frontend polling and UI updates
- **Bootstrap**: UI styling

## Configuration

The notification system is configured in `appsettings.json` or environment variables:

```json
{
  "AzureServiceBus": {
    "FullyQualifiedNamespace": "<namespace>.servicebus.windows.net",
    "NotificationQueueName": "contoso-university-notifications"
  }
}
```

In Azure, prefer app settings such as:

- `AzureServiceBus__FullyQualifiedNamespace=<namespace>.servicebus.windows.net`
- `AzureServiceBus__NotificationQueueName=contoso-university-notifications`

Grant the application's managed identity data-plane permissions for the queue, such as Azure Service Bus Data Sender and Azure Service Bus Data Receiver.

## Queue Details

- **Queue Name**: `contoso-university-notifications`
- **Authentication**: Managed identity through `DefaultAzureCredential`
- **Message Format**: JSON serialized notification objects
- **Message Metadata**: Queue messages use JSON content type and do not require a routing subject

## Usage

### For Administrators

1. Log in with an administrator account
2. Navigate to **Notifications** in the main menu to view the dashboard
3. Perform any CRUD operation on entities (Students, Courses, Instructors, Departments)
4. Watch for notifications appearing in the top-right corner
5. Notifications auto-dismiss after 1 minute or can be manually closed

### For Developers

To add notification support to a new controller:

1. Inherit from `BaseController` instead of `Controller`
2. Remove the private `SchoolContext db` declaration (handled by base class)
3. Call `SendEntityNotification()` after successful save operations:

```csharp
db.Students.Add(student);
db.SaveChanges();
SendEntityNotification("Student", student.ID.ToString(), EntityOperation.CREATE);
```

## Notification Types

- **CREATE**: Green notification for entity creation
- **UPDATE**: Blue notification for entity updates
- **DELETE**: Orange notification for entity deletion

## System Requirements

- .NET SDK matching the project target framework
- SQL Server LocalDB or configured SQL Server
- Azure Service Bus namespace and queue for deployed environments
- Managed identity or developer credential with queue send/receive permissions

## Testing the System

1. Access the **Notifications** dashboard from the admin menu
2. Click on any of the "Create new..." buttons provided
3. Complete a create/edit/delete operation
4. Observe the notification appearing in the top-right corner

## Troubleshooting

### Common Issues

1. **No notifications appearing**: Check browser console and `/Notifications/GetNotifications` network calls
2. **Service Bus authentication fails**: Verify managed identity is enabled and has send/receive permissions
3. **Queue not found**: Verify the configured queue exists in the configured namespace
4. **Local development credentials**: Sign in with a credential supported by `DefaultAzureCredential`, such as Azure CLI or Visual Studio

### Development Notes

- Notification failures are logged to debug output but do not affect user operations
- JavaScript polling occurs every 5 seconds
- Maximum of 5 notifications are displayed simultaneously

## Architecture Benefits

- **Decoupled**: Service Bus messaging keeps notification delivery independent from CRUD operations
- **Reliable**: Messages remain locked until completed and can be retried if processing fails
- **Scalable**: Can extend to multiple processors or administrator notification channels
- **Maintainable**: Clear separation between notification logic and business logic
