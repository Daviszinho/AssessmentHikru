#!/bin/bash
# Set environment variables
export TNS_ADMIN=/home/site/wwwroot/wallet
export ORACLE_HOME=/usr/lib/oracle/21/client64
export LD_LIBRARY_PATH=/usr/lib/oracle/21/client64/lib:$LD_LIBRARY_PATH

# Start the application
dotnet RestWebServices.dll
