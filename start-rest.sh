#!/bin/bash

# Colores para la salida
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Obtener el directorio del script
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
REST_DIR="$SCRIPT_DIR/RestWebServices"

# Verificar si el directorio existe
if [ ! -d "$REST_DIR" ]; then
    echo -e "${YELLOW}Error: No se encontró el directorio de servicios REST en $REST_DIR${NC}"
    exit 1
fi

echo -e "${GREEN}Iniciando servicios REST...${NC}"

# Navegar al directorio y ejecutar los servicios
cd "$REST_DIR"

# Verificar si dotnet está instalado
if ! command -v dotnet &> /dev/null; then
    echo -e "${YELLOW}Error: .NET SDK no está instalado. Por favor instálalo primero.${NC}"
    exit 1
fi

# Verificar si el proyecto ya está compilado
if [ ! -d "bin/Debug/net9.0" ] && [ ! -d "bin/Release/net9.0" ]; then
    echo -e "${YELLOW}Compilando el proyecto...${NC}"
    dotnet build
    
    if [ $? -ne 0 ]; then
        echo -e "${YELLOW}Error al compilar el proyecto.${NC}"
        exit 1
    fi
fi

echo -e "${GREEN}Ejecutando servicios REST en http://localhost:5000${NC}"
echo -e "${YELLOW}Presiona Ctrl+C para detener los servicios${NC}"

# Ejecutar el proyecto
dotnet run --no-build

# Si hay un error, mostrarlo
if [ $? -ne 0 ]; then
    echo -e "${YELLOW}Error al iniciar los servicios REST.${NC}"
    exit 1
fi
