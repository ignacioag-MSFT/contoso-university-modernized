# Setup and Testing Guide for Notification System

## Prerequisites

1. **Azure Service Bus access**:
   - Create or identify a Service Bus namespace.
   - Create the notification queue, for example `contoso-university-notifications`.
   - Grant the application managed identity Azure Service Bus Data Sender and Azure Service Bus Data Receiver roles for the queue or namespace.

2. **Local developer credentials**:
   - Sign in with Azure CLI, Visual Studio, or another credential supported by `DefaultAzureCredential`.
   - Ensure the signed-in identity has the same Service Bus data-plane permissions.

3. **Configuration**:
   - Set `AzureServiceBus__FullyQualifiedNamespace` to `<namespace>.servicebus.windows.net`.
   - Optionally set `AzureServiceBus__NotificationQueueName` if the queue name differs from `contoso-university-notifications`.
   - For local UI testing, use a developer identity with queue send and receive permissions.

## Building the Project

1. Open the solution in Visual Studio.
2. Restore NuGet packages if prompted.
3. Build the solution.

## Testing the Notification System

### Step 1: Run the Application
1. Press F5 to start debugging.
2. The application will launch in your default browser.

### Step 2: Login as Administrator
1. The application uses Windows Authentication.
2. Ensure your Windows user is in the administrator role.
3. You should see "Administrator" label next to your username.

### Step 3: Access Notification Dashboard
1. Click "Notifications" in the main navigation menu.
2. This page explains the notification system and provides test links.

### Step 4: Test Notifications
1. **Create a Student**:
   - Click "Students" → "Create New".
   - Fill in the form and submit.
   - Watch for a green notification in the top-right corner.

2. **Edit a Student**:
   - Go to Students list, click "Edit" on any student.
   - Make changes and save.
   - Watch for a blue notification.

3. **Delete a Student**:
   - Go to Students list, click "Delete" on any student.
   - Confirm deletion.
   - Watch for an orange notification.

4. **Test Other Entities**:
   - Repeat the same process for Courses, Instructors, and Departments.
   - Each operation should trigger appropriate notifications.

### Step 5: Verify Service Bus Queue
1. Open the Service Bus namespace in the Azure portal.
2. Open the configured queue.
3. Confirm active message count changes when operations produce notifications.

## Troubleshooting

### No Notifications Appearing
1. **Check Browser Console**: Press F12 and look for JavaScript errors.
2. **Check Network Tab**: Verify calls to `/Notifications/GetNotifications` are happening.
3. **Check Service Bus Queue**: Verify the queue exists and the application identity has permissions.

### Azure Service Bus Errors
1. **Authentication failed**:
   - Confirm managed identity is enabled for the host.
   - Confirm the identity has Service Bus data sender and receiver role assignments.

2. **Queue not found**:
   - Confirm `AzureServiceBus__NotificationQueueName` matches the provisioned queue name.
   - Confirm `AzureServiceBus__FullyQualifiedNamespace` points to the correct namespace.

3. **Local credential issues**:
   - Sign in using Azure CLI or Visual Studio with an identity that has queue permissions.

### JavaScript Not Loading
1. **Admin Role Check**: Ensure you're logged in as administrator.
2. **File Paths**: Verify `notifications.js` and `notifications.css` files exist.
3. **Browser Cache**: Clear cache and refresh.

## Configuration Notes

- **Queue Name**: Configured through `AzureServiceBus:NotificationQueueName`.
- **Namespace**: Configured through `AzureServiceBus:FullyQualifiedNamespace`.
- **Polling Interval**: JavaScript checks for new notifications every 5 seconds.
- **Auto-dismiss**: Notifications automatically disappear after 1 minute.
- **Max Notifications**: Maximum of 5 notifications shown simultaneously.

## Production Considerations

For production deployment:

1. **Managed identity**: Use a system-assigned or user-assigned managed identity.
2. **Least privilege**: Scope Service Bus role assignments to the queue when practical.
3. **Monitoring**: Monitor queue length, dead-letter count, and processing failures.
4. **Retry behavior**: Configure queue delivery count and dead-letter policy to match operational needs.

## Development Tips

- Notifications are designed to be non-blocking; send or receive failures won't break main operations.
- Debug output shows notification send/receive errors.
- Use notification dashboard to understand system behavior.
- Test with multiple admin users to verify isolation.
