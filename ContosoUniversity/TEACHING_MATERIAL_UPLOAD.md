# Teaching Material Image Upload Feature

This feature allows administrators to upload images for teaching materials (textbooks) associated with courses.

## Features

- **Image Upload**: Upload teaching material images when creating or editing courses
- **File Validation**: Supports JPG, JPEG, PNG, GIF, and BMP formats
- **Size Limits**: Maximum file size of 5MB per image
- **Secure Storage**: Images are stored in a private Azure Blob Storage container
- **Automatic Cleanup**: Images are automatically deleted when courses are removed
- **Unique Filenames**: Each uploaded image gets a unique filename to prevent conflicts

## Usage

### Creating a Course with Teaching Material Image

1. Navigate to the Courses section
2. Click "Create New" (Admin only)
3. Fill in the course details
4. In the "Teaching Material Image" section, click "Choose File"
5. Select an image file (JPG, JPEG, PNG, GIF, or BMP)
6. Click "Create" to save the course

### Editing a Course's Teaching Material Image

1. Navigate to the Courses section
2. Click "Edit" next to the course you want to modify
3. If a teaching material image already exists, it will be displayed
4. To change the image, click "Choose File" and select a new image
5. Click "Save" to update the course

### Viewing Teaching Material Images

- **Course List**: Small thumbnails (50x50px) are displayed in the courses index
- **Course Details**: Full-size images (max 300x300px) are displayed on the course details page

## Technical Details

### File Storage
- Images are stored in the Azure Blob Storage container configured by `Storage:TeachingMaterialsContainerName`
- Filenames follow the pattern: `course_{CourseID}_{GUID}.{extension}`
- Old images are automatically deleted when replaced
- **Important**: Uploaded images are stored outside the git repository in Azure Blob Storage

### Git Repository Management
- Actual uploaded images are not stored in version control:
  - Keep repository size manageable
  - Prevent sensitive content from being committed
  - Allow different environments to manage their own uploaded content in Azure
- When deploying to new environments, configure `Storage:ServiceUri` or `Storage:StorageAccountName` and grant the application identity Storage Blob Data Contributor access

### Database Schema
- New field: `TeachingMaterialImagePath` (VARCHAR(255)) added to the Course table
- Stores the Azure blob name for the uploaded image

### Security
- File type validation prevents uploading of non-image files
- File size validation prevents uploads larger than 5MB
- Only authenticated users with appropriate roles can upload images

### Authorization
- **Create/Upload**: Admin role required
- **Edit/Upload**: Admin or Teacher role required
- **View**: All authenticated users can view images
- **Delete**: Admin role required (deletes both course and associated image)

## Troubleshooting

### Common Issues

1. **"File too large" error**: Ensure your image is under 5MB
2. **"Invalid file type" error**: Only JPG, JPEG, PNG, GIF, and BMP files are supported
3. **Upload fails**: Check Azure Blob Storage configuration and verify the application identity has Storage Blob Data Contributor access to the storage account/container

### Configuration

The application validates teaching material images before upload and rejects files larger than 5MB. Configure ASP.NET Core hosting request-body limits separately if an environment needs to accept larger multipart requests.

## Deployment Considerations

### Initial Setup
1. Set `Storage:ServiceUri` or `Storage:StorageAccountName` for the target storage account
2. Set `Storage:TeachingMaterialsContainerName` if using a non-default container name
3. Grant the application identity Storage Blob Data Contributor access
4. Verify ASP.NET Core hosting request-body limits are appropriate for your hosting environment

### Azure RBAC Permissions
The application uses `DefaultAzureCredential` and needs Azure RBAC access to Blob Storage:
- **Local development**: sign in with Azure CLI or Visual Studio using an account with Storage Blob Data Contributor access
- **Azure App Service**: enable managed identity and grant Storage Blob Data Contributor on the storage account or container
- **Containers**: provide a managed identity/workload identity or other `DefaultAzureCredential`-supported identity

### Backup Strategy
Since uploaded images are not in version control, implement a backup strategy:
- Enable storage account backup, soft delete, versioning, or replication as appropriate
- Include the storage account/container in disaster recovery plans
- Document the restore process for disaster recovery

## Future Enhancements

Potential improvements for this feature:
- Image resizing and optimization
- Multiple image support per course
- Image gallery view
- Bulk upload functionality
- Image metadata support (alt text, captions)
