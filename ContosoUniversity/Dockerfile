# Multi-stage Dockerfile for ContosoUniversity ASP.NET Core 9.0 application
# This Dockerfile follows Azure Container Apps best practices with optimized layer caching

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file first for better layer caching
COPY ["ContosoUniversity.csproj", "./"]

# Restore dependencies
RUN dotnet restore "ContosoUniversity.csproj"

# Copy the rest of the source code
COPY . .

# Build the application in Release configuration
RUN dotnet build "ContosoUniversity.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "ContosoUniversity.csproj" -c Release -o /app/publish --no-restore

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Create non-root user for security best practices
RUN useradd -m -u 1001 aspnetuser && chown -R aspnetuser:aspnetuser /app
USER aspnetuser

# Copy published application from publish stage
COPY --from=publish /app/publish .

# Set environment variables for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port 8080 for Azure Container Apps
EXPOSE 8080

# Health check for Azure Container Apps
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "ContosoUniversity.dll"]
