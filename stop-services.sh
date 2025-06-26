#!/bin/bash

# Colores para la salida
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Deteniendo servicios...${NC}"

# Función para matar procesos por nombre
kill_process() {
    local process_name=$1
    local pids
    
    # Encontrar PIDs de los procesos
    pids=$(pgrep -f "$process_name")
    
    if [ -z "$pids" ]; then
        echo -e "${YELLOW}No se encontraron procesos de $process_name en ejecución.${NC}"
        return 1
    fi
    
    echo -e "${YELLOW}Deteniendo $process_name...${NC}"
    # Enviar señal SIGTERM (15) primero
    kill -15 $pids 2>/dev/null
    
    # Esperar un momento para que los procesos terminen
    sleep 2
    
    # Verificar si los procesos aún están en ejecución
    if pgrep -f "$process_name" >/dev/null; then
        echo -e "${YELLOW}Algunos procesos no respondieron a SIGTERM. Forzando terminación...${NC}"
        # Si los procesos no responden a SIGTERM, forzar con SIGKILL (9)
        kill -9 $pids 2>/dev/null
    fi
    
    echo -e "${GREEN}$process_name detenido correctamente.${NC}"
    return 0
}

# Detener la aplicación React (proceso de Vite/React)
kill_process "vite"
kill_process "node.*WebApp"

# Detener los servicios .NET
kill_process "dotnet.*RestWebServices"

# También verificar y matar cualquier proceso en los puertos comunes
echo -e "${YELLOW}Verificando puertos...${NC}"

# Puertos comunes a verificar (5000 para .NET, 3000/5173 para React)
PORTS=("5000" "3000" "5173")

for port in "${PORTS[@]}"; do
    pid=$(lsof -ti :$port)
    if [ ! -z "$pid" ]; then
        echo -e "${YELLOW}Matando proceso en el puerto $port (PID: $pid)${NC}"
        kill -9 $pid 2>/dev/null
    fi
done

echo -e "${GREEN}¡Todos los servicios han sido detenidos!${NC}"
