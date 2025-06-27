/*using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Lib.Repository.Entities;
using Lib.Repository.Repository.Commands;
using Lib.Repository.Repository.Queries;
using OracleConnectivity.Repository.Commands;
using OracleConnectivity.Repository.Queries;

namespace OracleConnectivity
{
    class Program
    {
        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
        }

        static async Task Main(string[] args)
        {
            try
            {
                // Configuración
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // Obtener cadena de conexión
                var connectionString = configuration.GetConnectionString("OracleConnection_Development");
                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("Error: No se encontró la cadena de conexión 'OracleConnection_Development' en la configuración.");
                    return;
                }

                // Crear instancias de los repositorios
                using var commandRepo = new PositionCommandRepository(connectionString);
                using var queryRepo = new PositionQueryRepository(connectionString);

                // Ejemplo: Obtener todas las posiciones
                Console.WriteLine("Obteniendo todas las posiciones...");
                var positions = await queryRepo.GetAllPositionsAsync();

                // Mostrar posiciones
                Console.WriteLine("\nPosiciones en la base de datos:");
                Console.WriteLine(new string('-', 100));
                Console.WriteLine($"{"ID",-5} | {"Título",-20} | {"Nivel",-10} | {"Departamento",-15} | {"Activo",-10} | {"Creado"}");
                Console.WriteLine(new string('-', 100));

                foreach (var position in positions)
                {
                    Console.WriteLine($"{position.Id,-5} | {Truncate(position.Title, 18),-20} | {position.DepartmentId,-15} | {(position.IsActive ? "Sí" : "No"),-10} | {position.CreatedAt:yyyy-MM-dd}");
                }

                // Ejemplo: Crear una nueva posición
                Console.WriteLine("\nCreando una nueva posición...");
                var newPosition = new Position
                {
                    Title = "Nueva Posición " + DateTime.Now.ToString("HHmmss"),
                    DepartmentId = 1,
                    Description = "Descripción de prueba",
                    IsActive = true
                };

                var newPositionId = await commandRepo.AddPositionAsync(newPosition);
                if (newPositionId.HasValue)
                {
                    Console.WriteLine($"Nueva posición creada con ID: {newPositionId}");
                    
                    // Obtener la nueva posición
                    var createdPosition = await queryRepo.GetPositionByIdAsync(newPositionId.Value);
                    if (createdPosition != null)
                    {
                        Console.WriteLine("\nDetalles de la nueva posición:");
                        Console.WriteLine($"Título: {createdPosition.Title}");
                        Console.WriteLine($"Departamento ID: {createdPosition.DepartmentId}");
                    }
                }
                else
                {
                    Console.WriteLine("Error al crear la nueva posición");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Detalles: {ex.InnerException.Message}");
                }
            }
            
            Console.WriteLine("\nPresione cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}*/