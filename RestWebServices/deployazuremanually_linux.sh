#!/bin/bash
dotnet clean
dotnet build
dotnet publish -c Release -o ./publish
cd publish && zip -r site.zip *
az webapp deployment source config-zip --resource-group HikruTestResourceGroup --name HikruOracle --src ./site.zip

#az webapp log tail --name HikruTestApp --resource-group HikruTestResourceGroup