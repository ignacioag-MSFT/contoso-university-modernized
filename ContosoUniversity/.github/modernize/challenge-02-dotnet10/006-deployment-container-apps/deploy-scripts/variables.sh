#!/bin/bash
# Configuration variables for ContosoUniversity Azure Container Apps deployment
# This file contains all the configuration parameters needed for deployment
# Update these values before running deployment scripts

# ============================================================================
# SUBSCRIPTION AND RESOURCE GROUP CONFIGURATION
# ============================================================================

# Azure Subscription ID where resources will be deployed
SUBSCRIPTION_ID=""

# Resource Group name where all resources will be created/deployed
RESOURCE_GROUP=""

# Azure Region for deployment (e.g., eastus, westus2, northcentralus)
REGION=""

# ============================================================================
# AZURE CONTAINER REGISTRY (ACR) CONFIGURATION
# ============================================================================

# ACR name (must be globally unique, alphanumeric only, 5-50 chars)
ACR_NAME=""

# ACR login server (auto-generated from ACR name)
ACR_LOGIN_SERVER="${ACR_NAME}.azurecr.io"

# ACR SKU (Basic, Standard, Premium)
ACR_SKU="Standard"

# ============================================================================
# AZURE CONTAINER APPS CONFIGURATION
# ============================================================================

# Container Apps Environment name
ACA_ENV_NAME=""

# Container App application name
ACA_APP_NAME="contosouniversity"

# Container App resource ID (format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.App/containerApps/{name})
# This will be populated after creation
ACA_APP_RESOURCE_ID=""

# Container Apps CPU cores (0.25, 0.5, 1.0)
ACA_CPU="0.5"

# Container Apps Memory (256Mi, 512Mi, 1Gi, 2Gi, 4Gi)
ACA_MEMORY="1Gi"

# Minimum number of replicas
ACA_MIN_REPLICAS="1"

# Maximum number of replicas
ACA_MAX_REPLICAS="3"

# Container port
CONTAINER_PORT="8080"

# ============================================================================
# DOCKER IMAGE CONFIGURATION
# ============================================================================

# Docker image name
DOCKER_IMAGE_NAME="contosouniversity"

# Docker image tag
DOCKER_IMAGE_TAG="latest"

# Full image URI
DOCKER_IMAGE_URI="${ACR_LOGIN_SERVER}/${DOCKER_IMAGE_NAME}:${DOCKER_IMAGE_TAG}"

# Path to Dockerfile
DOCKERFILE_PATH="./Dockerfile"

# Docker build context
DOCKER_BUILD_CONTEXT="."

# ============================================================================
# AZURE SQL DATABASE CONFIGURATION
# ============================================================================

# SQL Server name
SQL_SERVER_NAME=""

# SQL Server FQDN
SQL_SERVER_FQDN="${SQL_SERVER_NAME}.database.windows.net"

# Database name
SQL_DATABASE_NAME="ContosoUniversityNoAuthEFCore"

# SQL Server SKU (Standard, Premium, etc.)
SQL_SKU="Standard"

# SQL Database edition (S0, S1, S2, etc.)
SQL_EDITION="S0"

# Connection string format for Managed Identity
# Data Source={SERVER_NAME}.database.windows.net;Initial Catalog={DB_NAME};Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
SQL_CONNECTION_STRING_TEMPLATE="Data Source=${SQL_SERVER_FQDN};Initial Catalog=${SQL_DATABASE_NAME};Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# ============================================================================
# AZURE BLOB STORAGE CONFIGURATION
# ============================================================================

# Storage Account name
STORAGE_ACCOUNT_NAME=""

# Storage Account FQDN
STORAGE_ACCOUNT_FQDN="${STORAGE_ACCOUNT_NAME}.blob.core.windows.net"

# Blob container name
BLOB_CONTAINER_NAME="teaching-materials"

# Storage Account SKU (Standard_LRS, Standard_GRS, Standard_RAGRS, Premium_LRS)
STORAGE_SKU="Standard_LRS"

# Storage blob service URI
STORAGE_SERVICE_URI="https://${STORAGE_ACCOUNT_FQDN}"

# ============================================================================
# AZURE SERVICE BUS CONFIGURATION
# ============================================================================

# Service Bus Namespace name
SERVICE_BUS_NAMESPACE=""

# Service Bus Queue name
SERVICE_BUS_QUEUE="contoso-university-notifications"

# Service Bus Namespace FQDN
SERVICE_BUS_FQDN="${SERVICE_BUS_NAMESPACE}.servicebus.windows.net"

# Service Bus SKU (Basic, Standard, Premium)
SERVICE_BUS_SKU="Standard"

# ============================================================================
# MANAGED IDENTITY CONFIGURATION
# ============================================================================

# Enable system-assigned managed identity (true/false)
ENABLE_SYSTEM_IDENTITY="true"

# Optional: User-assigned managed identity name (leave empty to use system-assigned)
USER_ASSIGNED_IDENTITY_NAME=""

# Optional: User-assigned managed identity resource ID
# Format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/{name}
USER_ASSIGNED_IDENTITY_ID=""

# ============================================================================
# LOG ANALYTICS AND MONITORING
# ============================================================================

# Log Analytics Workspace name
LOG_ANALYTICS_WORKSPACE_NAME=""

# Log Analytics Workspace SKU (PerGB2018, etc.)
LOG_ANALYTICS_SKU="PerGB2018"

# Application Insights name (optional)
APP_INSIGHTS_NAME=""

# ============================================================================
# APPLICATION ENVIRONMENT VARIABLES
# ============================================================================

# ASP.NET Core Environment
ASPNETCORE_ENVIRONMENT="Production"

# Application settings (will be passed to container)
APP_SETTINGS_JSON_PATH="./appsettings.json"

# Additional environment variables (comma-separated, format: KEY=VALUE)
# Example: "DEBUG=false,LOG_LEVEL=Information"
ADDITIONAL_ENV_VARS=""

# ============================================================================
# DEPLOYMENT TRACKING
# ============================================================================

# Deployment name prefix (for tracking)
DEPLOYMENT_PREFIX="contosouniversity"

# Timestamp for deployment tracking
DEPLOYMENT_TIMESTAMP=$(date +%s)

# Deployment ID
DEPLOYMENT_ID="${DEPLOYMENT_PREFIX}-${DEPLOYMENT_TIMESTAMP}"

# ============================================================================
# HELPER VARIABLES (Auto-generated)
# ============================================================================

# Container App FQDN (populated after deployment)
ACA_FQDN=""

# Container App Health Check URL
HEALTH_CHECK_URL="https://${ACA_FQDN}/health"

# Bicep template path
BICEP_TEMPLATE_PATH="./main.bicep"

# Bicep parameters file
BICEP_PARAMS_FILE="./main.bicepparam"

# ============================================================================
# END OF CONFIGURATION
# ============================================================================

echo "Configuration variables loaded from variables.sh"
