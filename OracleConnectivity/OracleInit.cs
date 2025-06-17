using Oracle.ManagedDataAccess.Client;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hikru.Assessment.OracleConnectivity
{   
    public static class OracleInit
    {   
        public static async Task<OracleConnection> ConnectToOracleAsync()
        {
            // Configuración de la conexión a Oracle Cloud
            string tnsAdmin = "C:\\Users\\dpena\\Documents\\Projects\\OracleTimeQuery\\wallet";
        
            // Configurar el entorno de Oracle
            Environment.SetEnvironmentVariable("TNS_ADMIN", tnsAdmin);
        
            // Usar el TNS name del archivo tnsnames.ora
            string tnsName = "oracletest_medium";
        
            // Usar credenciales explícitas en lugar de autenticación con wallet
            string username = "admin";
            string password = "DavisOracle25!";
        
            // Configurar la cadena de conexión
            string connectionString = 
                $"User Id={username};" +
                $"Password={password};" +
                "Connection Timeout=30;" +
                $"TNS_ADMIN={tnsAdmin};" +
                $"Data Source={tnsName};";
            
            // Asegurarse de que el directorio del wallet exista
            if (!System.IO.Directory.Exists(tnsAdmin))
            {
                Console.WriteLine($"⚠️ Advertencia: El directorio del wallet no existe: {tnsAdmin}");
                Console.WriteLine("Por favor, asegúrate de haber descargado el wallet de Oracle Cloud y colocado los archivos en esta ubicación.");
            }

            Console.WriteLine("Configuración de conexión:");
            Console.WriteLine(connectionString.Replace("DavisOracle25!", "********"));

            Console.WriteLine("Iniciando conexión a Oracle...");
            
            // Crear la conexión
            var connection = new OracleConnection(connectionString);
            Console.WriteLine("Intentando abrir la conexión...");
            
            try
            {
                // Establecer un tiempo de espera para la conexión
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    // Intentar abrir la conexión de forma asíncrona con tiempo de espera
                    var openTask = connection.OpenAsync(cts.Token);
                    var timeoutTask = Task.Delay(Timeout.Infinite, cts.Token);
                    
                    var completedTask = await Task.WhenAny(openTask, timeoutTask);
                    if (completedTask == timeoutTask)
                    {
                        throw new TimeoutException("Tiempo de espera agotado al intentar conectar con la base de datos.");
                    }
                    
                    // Si llegamos aquí, openTask se completó
                    await openTask;
                    Console.WriteLine("Conexión establecida correctamente.");
                    return connection;
                }
            }
            catch (Exception ex)
            {
                // Si hay un error, asegurarse de limpiar la conexión
                connection.Dispose();
                Console.WriteLine($"Error al abrir la conexión: {ex.Message}");
                throw new Exception("No se pudo establecer la conexión con la base de datos.", ex);
            }
        }
    }
}