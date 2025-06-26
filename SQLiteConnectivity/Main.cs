using Microsoft.Data.Sqlite;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Lib.Repository.Entities;
using Lib.Repository.Repository.Queries;
using SQLiteConnectivity.Repository.Queries;

public class Program
{
    private const string DatabaseFileName = "db_hikru_test.db";
    private const string ScriptFileName = "InitializeDatabase.sql";

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
    }

    private static string FindScriptFile()
    {
        // Try multiple possible locations for the script file
        var possiblePaths = new[]
        {
            ScriptFileName, // Current directory
            Path.Combine("Scripts", ScriptFileName), // Scripts subdirectory
            Path.Combine(Directory.GetCurrentDirectory(), ScriptFileName), // Full path in current directory
            Path.Combine(Directory.GetCurrentDirectory(), "Scripts", ScriptFileName), // Full path in Scripts subdirectory
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ScriptFileName), // Output directory
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", ScriptFileName) // Output directory Scripts subdirectory
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                Console.WriteLine($"Found script file at: {path}");
                return path;
            }
        }


        // Throw an exception with all the paths that were checked
        var searchedPaths = string.Join(Environment.NewLine, possiblePaths);
        throw new FileNotFoundException($"Could not find script file '{ScriptFileName}'. Searched in the following locations:\n{searchedPaths}");
    }

    private static async Task InitializeDatabaseAsync(string connectionString)
    {
        try
        {
            // Find the script file
            string scriptPath = FindScriptFile();
            if (string.IsNullOrEmpty(scriptPath))
            {
                Console.WriteLine($"Warning: SQL script file not found. Searched in:");
                Console.WriteLine($"  - Current directory: {Directory.GetCurrentDirectory()}");
                Console.WriteLine($"  - Base directory: {AppDomain.CurrentDomain.BaseDirectory}");
                Console.WriteLine($"  - Scripts subdirectory");
                return;
            }

            Console.WriteLine($"Reading SQL script from: {scriptPath}");
            string sqlScript = await File.ReadAllTextAsync(scriptPath);
            
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Executing database initialization script...");
                
                // Execute the entire script as one command
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sqlScript;
                    await command.ExecuteNonQueryAsync();
                }
                
                Console.WriteLine("Database initialization completed successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing database: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    public static async Task Main(string[] args)
    {
        try
        {
            // Create database file if it doesn't exist
            if (!File.Exists(DatabaseFileName))
            {
                Console.WriteLine($"Creating database file: {DatabaseFileName}");
                File.Create(DatabaseFileName).Close();
            }

            // Create a connection to the SQLite database
            string connectionString = $"Data Source={DatabaseFileName}";

            // Initialize the database with tables and sample data
            await InitializeDatabaseAsync(connectionString);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine($"Successfully connected to database: {DatabaseFileName}");
                
                // Create and use PositionQueryRepository
                IPositionQueryRepository queryRepository = new PositionQueryRepository(connectionString);
                var positions = await queryRepository.GetAllPositionsAsync();

                // Display positions
                Console.WriteLine("\nPositions in the database:");
                Console.WriteLine(new string('-', 100));
                Console.WriteLine($"{"ID",-5} | {"Title",-20} | {"Status",-15} | {"Location",-15} | {"Department",-20} | {"Recruiter",-20}");
                Console.WriteLine(new string('-', 100));

                foreach (var position in positions)
                {
                    Console.WriteLine($"{position.Id,-5} | {Truncate(position.Title, 18),-20} | {position.Status ?? "N/A",-15} | {position.Location ?? "N/A",-15} | {Truncate(position.DepartmentName ?? "N/A", 18),-20} | {Truncate(position.RecruiterName ?? "N/A", 18),-20}");
                }

                // Get and display position with ID 1 if available
                if (positions.Any())
                {
                    int firstPositionId = positions.First().Id;
                    Console.WriteLine($"\nFetching position with ID {firstPositionId}:");
                    var positionById = await queryRepository.GetPositionByIdAsync(firstPositionId);
                    if (positionById != null)
                    {
                        Console.WriteLine(new string('-', 100));
                        Console.WriteLine($"{"ID",-5} | {"Title",-20} | {"Status",-15} | {"Location",-15} | {"Department",-20} | {"Recruiter",-20}");
                        Console.WriteLine(new string('-', 100));
                        Console.WriteLine($"{positionById.Id,-5} | {Truncate(positionById.Title, 18),-20} | {positionById.Status ?? "N/A",-15} | {positionById.Location ?? "N/A",-15} | {Truncate(positionById.DepartmentName ?? "N/A", 18),-20} | {Truncate(positionById.RecruiterName ?? "N/A", 18),-20}");
                    }
                    else
                    {
                        Console.WriteLine($"Position with ID {firstPositionId} not found.");
                    }
                }
                
                connection.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
