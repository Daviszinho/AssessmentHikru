#!/bin/bash

# Script para compilar todos los proyectos de la solución
# Colores para la salida
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Función para mostrar mensajes de éxito
success() {
    echo -e "${GREEN}[ÉXITO]${NC} $1"
}

# Función para mostrar mensajes de error
error() {
    echo -e "${RED}[ERROR]${NC} $1"
    exit 1
}

# Función para mostrar mensajes informativos
info() {
    echo -e "${YELLOW}[INFO]${NC} $1"
}

# Directorio raíz del proyecto
ROOT_DIR=$(pwd)
info "Directorio raíz: $ROOT_DIR"

# Lista de proyectos a compilar (en el orden correcto de dependencias)
PROJECTS=(
    "Lib.Repository/Lib.Repository.csproj"
    "SQLiteConnectivity/SQLiteConnectivity.csproj"
    "OracleConnectivity/OracleTimeQuery.csproj"
    "RestWebServices/RestWebServices.csproj"
    "API.Test/API.Test.csproj"
)

# Verificar si dotnet está instalado
if ! command -v dotnet &> /dev/null; then
    error "No se encontró el comando 'dotnet'. Asegúrate de tener .NET SDK instalado."
fi

info "Iniciando la compilación de la solución..."

# Compilar cada proyecto
for PROJECT in "${PROJECTS[@]}"; do
    PROJECT_PATH="$ROOT_DIR/$PROJECT"
    
    if [ ! -f "$PROJECT_PATH" ]; then
        error "No se encontró el proyecto: $PROJECT"
    fi
    
    info "Compilando: $PROJECT"
    
    # Navegar al directorio del proyecto y ejecutar dotnet build
    cd "$(dirname "$PROJECT_PATH")" || error "No se pudo cambiar al directorio: $(dirname "$PROJECT_PATH")"
    
    dotnet build --nologo -c Release -v minimal
    
    # Verificar el código de salida del comando anterior
    if [ $? -ne 0 ]; then
        error "Error al compilar el proyecto: $PROJECT"
    fi
    
    # Volver al directorio raíz
    cd "$ROOT_DIR" || error "No se pudo volver al directorio raíz"
    
    success "Proyecto compilado correctamente: $PROJECT"
done

# Mensaje final
success "¡Todos los proyectos se han compilado correctamente!"
exit 0
