#!/bin/bash

# Color codes for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}[INFO] Limpiando la solución...${NC}"

# Remove bin and obj directories
echo -e "${YELLOW}[INFO] Eliminando directorios bin y obj...${NC}"
find . -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} + 2>/dev/null || true

# Remove .vs directory if exists
if [ -d ".vs" ]; then
    echo -e "${YELLOW}[INFO] Eliminando directorio .vs...${NC}"
    rm -rf .vs
fi

# Remove any .DS_Store files (macOS)
find . -name ".DS_Store" -delete 2>/dev/null || true

# Remove any .user files
find . -name "*.user" -delete 2>/dev/null || true

# Remove any .suo files
find . -name "*.suo" -delete 2>/dev/null || true

echo -e "${GREEN}[ÉXITO] Limpieza completada.${NC}"
