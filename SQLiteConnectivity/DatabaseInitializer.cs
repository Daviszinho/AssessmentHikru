using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace SQLiteConnectivity
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeDatabaseAsync(string connectionString)
        {
            try
            {
                // Create the database file if it doesn't exist
                var dbPath = GetDatabasePath(connectionString);
                if (!File.Exists(dbPath))
                {
                    // Create the directory if it doesn't exist
                    var directory = Path.GetDirectoryName(dbPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Create the database file
                    File.WriteAllBytes(dbPath, Array.Empty<byte>());
                    
                    // Execute the initialization script
                    await ExecuteSqlScript(connectionString, "Scripts/InitializeDatabase.sql");
                }
                else
                {
                    // Check if tables exist, if not, run the initialization script
                    if (!await CheckIfTablesExist(connectionString))
                    {
                        await ExecuteSqlScript(connectionString, "Scripts/InitializeDatabase.sql");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
                throw;
            }
        }

        private static async Task<bool> CheckIfTablesExist(string connectionString)
        {
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT count(*) 
                FROM sqlite_master 
                WHERE type='table' 
                AND (name='Position' OR name='Department' OR name='Recruiter')";
                
            var tableCount = Convert.ToInt32(await command.ExecuteScalarAsync());
            return tableCount == 3; // All three tables should exist
        }

        private static async Task ExecuteSqlScript(string connectionString, string scriptPath)
        {
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, scriptPath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"SQL script not found: {fullPath}");
            }

            var script = await File.ReadAllTextAsync(fullPath);
            
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            
            // Split the script into individual commands
            var commands = script.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var cmdText in commands)
            {
                if (string.IsNullOrWhiteSpace(cmdText) || cmdText.Trim().StartsWith("--"))
                    continue;
                    
                using var command = connection.CreateCommand();
                command.CommandText = cmdText.Trim();
                await command.ExecuteNonQueryAsync();
            }
        }

        private static string GetDatabasePath(string connectionString)
        {
            var builder = new SqliteConnectionStringBuilder(connectionString);
            if (builder.DataSource == ":memory:")
                return ":memory:";
                
            // Handle relative paths
            var dataSource = builder.DataSource;
            if (!Path.IsPathRooted(dataSource))
            {
                dataSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dataSource);
            }
            
            return dataSource;
        }
    }
}
