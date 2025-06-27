#!/bin/bash

# Script de despliegue para Azure con configuración Oracle
# Este script asegura que los archivos del wallet se copien correctamente

echo "=== Despliegue a Azure con configuración Oracle ==="

# Variables de configuración
APP_NAME="hikrutestapp"
RESOURCE_GROUP="your-resource-group"
BUILD_CONFIG="Release"

echo "Compilando en modo Release..."
dotnet build RestWebServices/RestWebServices.csproj -c $BUILD_CONFIG

echo "Verificando archivos del wallet..."
if [ -d "RestWebServices/bin/$BUILD_CONFIG/net9.0/wallet" ]; then
    echo "✓ Directorio wallet encontrado en output"
    ls -la RestWebServices/bin/$BUILD_CONFIG/net9.0/wallet/
else
    echo "✗ Directorio wallet no encontrado en output"
    exit 1
fi

echo "Verificando archivo sqlnet.ora (debería ser copia de sqlnet.prod.ora)..."
if [ -f "RestWebServices/bin/$BUILD_CONFIG/net9.0/wallet/sqlnet.ora" ]; then
    echo "✓ sqlnet.ora encontrado (configuración de producción)"
    # Verificar que no es el archivo de desarrollo
    if grep -q "AssessmentHikru" "RestWebServices/bin/$BUILD_CONFIG/net9.0/wallet/sqlnet.ora"; then
        echo "⚠️  ADVERTENCIA: sqlnet.ora contiene ruta de desarrollo"
    else
        echo "✓ sqlnet.ora contiene configuración de producción"
    fi
else
    echo "✗ sqlnet.ora no encontrado"
    exit 1
fi

echo "Verificando que sqlnet.ora de desarrollo NO esté presente..."
if [ -f "RestWebServices/bin/$BUILD_CONFIG/net9.0/wallet/sqlnet.ora" ] && grep -q "AssessmentHikru" "RestWebServices/bin/$BUILD_CONFIG/net9.0/wallet/sqlnet.ora"; then
    echo "✗ ERROR: sqlnet.ora de desarrollo está presente en producción"
    exit 1
else
    echo "✓ sqlnet.ora de desarrollo NO está presente"
fi

echo "Publicando aplicación..."
dotnet publish RestWebServices/RestWebServices.csproj -c $BUILD_CONFIG -o ./publish

echo "Verificando archivos en publish..."
if [ -d "publish/wallet" ]; then
    echo "✓ Wallet copiado a publish"
    ls -la publish/wallet/
else
    echo "✗ Wallet no encontrado en publish"
    exit 1
fi

echo "Desplegando a Azure..."
# Comando de despliegue a Azure (ajusta según tu configuración)
# az webapp deployment source config-zip --resource-group $RESOURCE_GROUP --name $APP_NAME --src publish.zip

echo "Configurando variables de entorno en Azure..."
# az webapp config appsettings set --name $APP_NAME --resource-group $RESOURCE_GROUP --settings ASPNETCORE_ENVIRONMENT=Production

echo "Despliegue completado. Verifica que la aplicación esté funcionando correctamente."
echo "URL: https://$APP_NAME.azurewebsites.net" 