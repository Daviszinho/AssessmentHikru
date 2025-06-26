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

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
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
