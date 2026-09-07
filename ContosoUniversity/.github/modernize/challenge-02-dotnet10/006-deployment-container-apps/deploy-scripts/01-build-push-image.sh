#!/bin/bash
# Step 1: Build and Push Docker image to Azure Container Registry
# This script builds the Docker image and pushes it to ACR

set -e  # Exit on error

# Source configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/variables.sh"

echo "=========================================="
echo "Step 1: Build and Push Docker Image"
echo "=========================================="
echo ""

# Validate required variables
if [ -z "$ACR_NAME" ] || [ -z "$ACR_LOGIN_SERVER" ]; then
    echo "❌ Error: ACR_NAME and ACR_LOGIN_SERVER must be configured in variables.sh"
    exit 1
fi

if [ -z "$DOCKER_IMAGE_NAME" ] || [ -z "$DOCKER_IMAGE_TAG" ]; then
    echo "❌ Error: DOCKER_IMAGE_NAME and DOCKER_IMAGE_TAG must be configured"
    exit 1
fi

echo "📋 Configuration:"
echo "   ACR Name: $ACR_NAME"
echo "   ACR Login Server: $ACR_LOGIN_SERVER"
echo "   Image Name: $DOCKER_IMAGE_NAME"
echo "   Image Tag: $DOCKER_IMAGE_TAG"
echo "   Full URI: $DOCKER_IMAGE_URI"
echo ""

# Step 1: Login to ACR
echo "🔐 Step 1/3: Logging in to Azure Container Registry..."
if az acr login --name "$ACR_NAME"; then
    echo "✅ Successfully logged in to ACR"
else
    echo "❌ Error: Failed to login to ACR"
    exit 1
fi
echo ""

# Step 2: Build image using ACR remote build
echo "🔨 Step 2/3: Building Docker image in ACR..."
if az acr build \
    --registry "$ACR_NAME" \
    --image "${DOCKER_IMAGE_NAME}:${DOCKER_IMAGE_TAG}" \
    --file "$DOCKERFILE_PATH" \
    "$DOCKER_BUILD_CONTEXT"; then
    echo "✅ Docker image built successfully in ACR"
else
    echo "❌ Error: Failed to build Docker image"
    exit 1
fi
echo ""

# Step 3: Verify image in registry
echo "🔍 Step 3/3: Verifying image in registry..."
if az acr repository show \
    --name "$ACR_NAME" \
    --image "${DOCKER_IMAGE_NAME}:${DOCKER_IMAGE_TAG}"; then
    echo "✅ Image verified in ACR"
else
    echo "❌ Error: Image not found in registry"
    exit 1
fi
echo ""

echo "=========================================="
echo "✅ Build and Push Complete"
echo "=========================================="
echo ""
echo "Docker Image Details:"
echo "   Registry: $DOCKER_IMAGE_URI"
echo "   Built: $(date)"
echo ""
echo "Next Step: Execute 02-deploy-container-app.sh"
