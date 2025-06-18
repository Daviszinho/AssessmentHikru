#!/bin/bash

# Set variables
RESOURCE_GROUP="HikruTestResourceGroup"
APP_NAME="HikruTestApp"
ZIP_FILE="app.zip"

# Navigate to the project directory
cd "$(dirname "$0")/../RestWebServices/RestWebServices"

# Create a temp directory for deployment
TEMP_DIR="$PWD/deploy_temp"
mkdir -p "$TEMP_DIR"

# Copy all files to the temp directory
echo "Copying files to temp directory..."
cp -r * "$TEMP_DIR/"

# Ensure the wallet directory exists in the temp directory
WALLET_SOURCE="$PWD/../wallet"
WALLET_TARGET="$TEMP_DIR/wallet"

if [ -d "$WALLET_SOURCE" ]; then
    echo "Copying wallet files..."
    mkdir -p "$WALLET_TARGET"
    cp -r "$WALLET_SOURCE/"* "$WALLET_TARGET/"
    
    # List the wallet files for verification
    echo "Wallet files copied:"
    ls -la "$WALLET_TARGET"
else
    echo "Warning: Wallet directory not found at $WALLET_SOURCE"
fi

# Create the zip file
echo "Creating deployment package..."
cd "$TEMP_DIR/.."
zip -r "$ZIP_FILE" "$(basename $TEMP_DIR)/"*

# Deploy to Azure
echo "Deploying to Azure App Service..."
az webapp deployment source config-zip \
    --resource-group "$RESOURCE_GROUP" \
    --name "$APP_NAME" \
    --src "$ZIP_FILE"

# Restart the app service
echo "Restarting App Service..."
az webapp restart --name "$APP_NAME" --resource-group "$RESOURCE_GROUP"

# Clean up
rm -rf "$TEMP_DIR"
rm -f "$ZIP_FILE"

echo "Deployment complete!"
