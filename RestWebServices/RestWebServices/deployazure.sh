#!/bin/bash
set -e  # Exit on error
echo "=== Starting deployment to Azure ==="

# Build the application
echo "=== Building the application... ==="
dotnet clean
rm -rf ./publish
dotnet publish -c Release -o ./publish

# Copy wallet files to the publish directory
echo "=== Copying wallet files... ==="
mkdir -p ./publish/wallet
cp -r ../../OracleConnectivity/wallet/* ./publish/wallet/

# Create a .deployment file to ensure the build happens on the server
echo "=== Creating .deployment file... ==="
echo '[config]' > ./publish/.deployment
echo 'SCM_DO_BUILD_DURING_DEPLOYMENT = true' >> ./publish/.deployment

# Create a startup script to set environment variables
echo "=== Creating startup script... ==="
cat > ./publish/startup.sh << 'EOL'
#!/bin/bash
# Set environment variables
export TNS_ADMIN=/home/site/wwwroot/wallet
export ORACLE_HOME=/usr/lib/oracle/21/client64
export LD_LIBRARY_PATH=/usr/lib/oracle/21/client64/lib:$LD_LIBRARY_PATH

# Start the application
dotnet RestWebServices.dll
EOL

# Make the startup script executable
chmod +x ./publish/startup.sh

# Create a custom startup command in the application settings
echo "=== Updating Azure app settings... ==="
az webapp config set \
  --resource-group HikruTestResourceGroup \
  --name HikruTestApp \
  --startup-file "startup.sh"

# Set the environment variables in Azure App Service
echo "=== Setting environment variables... ==="
az webapp config appsettings set \
  --resource-group HikruTestResourceGroup \
  --name HikruTestApp \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    TNS_ADMIN=/home/site/wwwroot/wallet \
    ORACLE_HOME=/usr/lib/oracle/21/client64 \
    LD_LIBRARY_PATH=/usr/lib/oracle/21/client64/lib

# Deploy the application
echo "=== Deploying application... ==="
cd publish
zip -r site.zip .

# Use the new deployment command instead of the deprecated one
echo "=== Starting deployment... ==="
az webapp deploy \
  --resource-group HikruTestResourceGroup \
  --name HikruTestApp \
  --src-path ./site.zip \
  --type zip \
  --clean true \
  --restart true

echo "=== Deployment completed! ==="
echo "Your app is now live at: https://hikrutestapp.azurewebsites.net"
echo "To view logs, run: az webapp log tail --name HikruTestApp --resource-group HikruTestResourceGroup"
