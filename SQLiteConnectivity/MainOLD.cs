using Microsoft.Data.Sqlite;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using SQLiteConnectivity.Repository;
using Lib.Repository.Entities;

public class ProgramOLD
{

 private const string DatabaseFileName = "db_hikru_test.db";

    
    
    /*private const string DatabaseFileName = "db_hikru_test.db";

     public static void Main(string[] args)
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
                
                // Create Department table if it doesn't exist
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Department (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            NAME TEXT NOT NULL
                        )";
                    
                    command.ExecuteNonQuery();
                    Console.WriteLine("Department table created or already exists");
                }
                
                // Create Recruiter table if it doesn't exist
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Recruiter (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            NAME TEXT NOT NULL
                        )";
                    
                    command.ExecuteNonQuery();
                    Console.WriteLine("Recruiter table created or already exists");
                }

                // Create Position table if it doesn't exist
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Position (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            TITLE TEXT NOT NULL,
                            DESCRIPTION TEXT NOT NULL,
                            LOCATION TEXT NOT NULL,
                            STATUS TEXT NOT NULL,
                            RECRUITERID INTEGER NOT NULL,
                            DEPARTMENTID INTEGER NOT NULL,
                            BUDGET REAL,
                            CLOSINGDATE TEXT,
                            CREATEDAT TEXT DEFAULT CURRENT_TIMESTAMP,
                            UPDATEDAT TEXT DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (RECRUITERID) REFERENCES Recruiter(ID) ON DELETE CASCADE,
                            FOREIGN KEY (DEPARTMENTID) REFERENCES Department(ID) ON DELETE CASCADE
                        )";
                    
                    command.ExecuteNonQuery();
                    Console.WriteLine("Position table created or already exists");
                }

                // Create PositionDetails view that joins all three tables
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE VIEW IF NOT EXISTS PositionDetails AS
                        SELECT 
                            p.ID as PositionID,
                            p.TITLE as PositionTitle,
                            p.DESCRIPTION as PositionDescription,
                            p.LOCATION as PositionLocation,
                            p.STATUS as PositionStatus,
                            p.BUDGET as PositionBudget,
                            p.CLOSINGDATE as PositionClosingDate,
                            p.CREATEDAT as PositionCreatedAt,
                            p.UPDATEDAT as PositionUpdatedAt,
                            r.ID as RecruiterID,
                            r.NAME as RecruiterName,
                            d.ID as DepartmentID,
                            d.NAME as DepartmentName
                        FROM Position p
                        JOIN Recruiter r ON p.RECRUITERID = r.ID
                        JOIN Department d ON p.DEPARTMENTID = d.ID";
                    
                    command.ExecuteNonQuery();
                    Console.WriteLine("PositionDetails view created or already exists");
                }
                
                connection.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }*/
}

