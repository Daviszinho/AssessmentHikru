using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Threading.Tasks;

class Program
{
    // Punto de entrada principal
    static async Task Main(string[] args)
    {
        try
        {
            await ConnectToOracleAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error inesperado: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("\nPresiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }

    static async Task ConnectToOracleAsync()
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

        try
        {
            Console.WriteLine("Iniciando conexión a Oracle...");
            
            // Crear y abrir la conexión
            using (var connection = new OracleConnection(connectionString))
            {
                Console.WriteLine("Intentando abrir la conexión...");
                
                // Establecer un tiempo de espera más corto para la conexión
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    try
                    {
                        // Intentar abrir la conexión de forma asíncrona con tiempo de espera
                        var openTask = connection.OpenAsync();
                        var timeoutTask = Task.Delay(Timeout.Infinite, cts.Token);
                        
                        var completedTask = await Task.WhenAny(openTask, timeoutTask);
                        if (completedTask == timeoutTask)
                        {
                            throw new TimeoutException("Tiempo de espera agotado al intentar conectar con la base de datos.");
                        }
                        
                        // Si llegamos aquí, openTask se completó
                        await openTask;
                        
                        Console.WriteLine("Conexión exitosa al servidor Oracle");
                        
                        // Crear un comando para consultar la fecha y hora del servidor
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT TO_CHAR(SYSDATE, 'DD-MON-YYYY HH24:MI:SS') AS FECHA_HORA FROM DUAL";
                            
                            Console.WriteLine("Ejecutando consulta...");
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    string fechaHoraServidor = reader.GetString(0);
                                    Console.WriteLine($"✅ La fecha y hora del servidor Oracle es: {fechaHoraServidor}");
                                }
                            }
                        }
                    }
                    catch (OracleException ex)
                    {
                        Console.WriteLine($"Error de Oracle (Código: {ex.Number}): {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"Detalles internos: {ex.InnerException.Message}");
                        }
                        
                        // Mostrar información adicional sobre el error de conexión
                        if (ex.Number >= 2000 && ex.Number < 3000)
                        {
                            Console.WriteLine("\nPosibles causas:");
                            Console.WriteLine("1. La base de datos no está disponible");
                            Console.WriteLine("2. Problemas de red o firewall");
                            Console.WriteLine("3. Credenciales incorrectas");
                            Console.WriteLine("4. El servicio no está configurado correctamente");
                        }
                        
                        throw;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.GetType().Name}");
            Console.WriteLine($"Mensaje: {ex.Message}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"\nDetalles internos:");
                Console.WriteLine($"Tipo: {ex.InnerException.GetType().Name}");
                Console.WriteLine($"Mensaje: {ex.InnerException.Message}");
            }
            
            Console.WriteLine("\nSolución de problemas:");
            Console.WriteLine("1. Verifica que la base de datos esté en línea y accesible");
            Console.WriteLine("2. Verifica tu conexión a internet");
            Console.WriteLine("3. Verifica las credenciales y la cadena de conexión");
            Console.WriteLine("4. Intenta desactivar temporalmente el firewall");
        }
    }
}
