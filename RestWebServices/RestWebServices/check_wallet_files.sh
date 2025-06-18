#!/bin/bash
# Get the publishing profile for the web app
echo "Getting publishing profile..."
pubxml=$(az webapp deployment list-publishing-profiles \
  --name HikruTestApp \
  --resource-group HikruTestResourceGroup \
  --query "[?publishMethod=='MSDeploy'].publishUrl" -o tsv)

# Get the SCM URL
scm_url="https://$pubxml.scm.azurewebsites.net/api/command"
echo "SCM URL: $scm_url"

# Get the basic auth credentials
username=$(az webapp deployment list-publishing-credentials \
  --name HikruTestApp \
  --resource-group HikruTestResourceGroup \
  --query scmUri -o tsv | cut -d'@' -f1 | cut -d'/' -f3)
password=$(az webapp deployment list-publishing-credentials \
  --name HikruTestApp \
  --resource-group HikruTestResourceGroup \
  --query publishingPassword -o tsv)

echo "Checking wallet files in Azure..."

# List the contents of the wallet directory
curl -u "$username:$password" \
  -X POST \
  -H "Content-Type: application/json" \
  -d '{"command": "ls -la /home/site/wwwroot/wallet", "dir": "/home/site/wwwroot"}' \
  "https://hikrutestapp.scm.azurewebsites.net/api/command"

# Check if the wallet files exist
echo -e "\n\nChecking if wallet files exist..."
curl -u "$username:$password" \
  -X POST \
  -H "Content-Type: application/json" \
  -d '{"command": "ls -la /home/site/wwwroot/wallet/ | grep -E \"cwallet.sso|ewallet.p12|ewallet.pem|keystore.jks|ojdbc.properties|sqlnet.ora|tnsnames.ora|truststore.jks\"", "dir": "/home/site/wwwroot"}' \
  "https://hikrutestapp.scm.azurewebsites.net/api/command"

# Check the contents of sqlnet.ora
echo -e "\n\nChecking sqlnet.ora contents..."
curl -u "$username:$password" \
  -X POST \
  -H "Content-Type: application/json" \
  -d '{"command": "cat /home/site/wwwroot/wallet/sqlnet.ora", "dir": "/home/site/wwwroot"}' \
  "https://hikrutestapp.scm.azurewebsites.net/api/command"

# Check the contents of tnsnames.ora
echo -e "\n\nChecking tnsnames.ora contents..."
curl -u "$username:$password" \
  -X POST \
  -H "Content-Type: application/json" \
  -d '{"command": "cat /home/site/wwwroot/wallet/tnsnames.ora", "dir": "/home/site/wwwroot"}' \
  "https://hikrutestapp.scm.azurewebsites.net/api/command"
