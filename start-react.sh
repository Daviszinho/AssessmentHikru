#!/bin/bash

# Colores para la salida
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Obtener el directorio del script
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
REACT_APP_DIR="$SCRIPT_DIR/WebApp"

# Verificar si el directorio existe
if [ ! -d "$REACT_APP_DIR" ]; then
    echo -e "${YELLOW}Error: No se encontró el directorio de la aplicación React en $REACT_APP_DIR${NC}"
    exit 1
fi

echo -e "${GREEN}Iniciando la aplicación React...${NC}"

# Navegar al directorio de la aplicación React
cd "$REACT_APP_DIR"

# Verificar si npm está instalado
if ! command -v npm &> /dev/null; then
    echo -e "${YELLOW}Error: Node.js y npm no están instalados. Por favor instálalos primero.${NC}"
    exit 1
fi

# Instalar dependencias si no existen
if [ ! -d "node_modules" ]; then
    echo -e "${YELLOW}Instalando dependencias...${NC}"
    npm install
    
    if [ $? -ne 0 ]; then
        echo -e "${YELLOW}Error al instalar las dependencias.${NC}"
        exit 1
    fi
fi

echo -e "${GREEN}Iniciando la aplicación React en modo desarrollo...${NC}
echo -e "${YELLOW}Presiona Ctrl+C para detener la aplicación${NC}"

# Iniciar la aplicación en modo desarrollo
npm run dev

# Si hay un error, mostrarlo
if [ $? -ne 0 ]; then
    echo -e "${YELLOW}Error al iniciar la aplicación React.${NC}"
    exit 1
fi
