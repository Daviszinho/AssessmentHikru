using Oracle.ManagedDataAccess.Client;
using System;
using System.Threading.Tasks;
using Hikru.Assessment.OracleConnectivity.Models;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Linq;

namespace Hikru.Assessment.OracleConnectivity
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Fetching positions from database...");
                
                using (var oracleQuery = new OracleQuery())
                {
                    // Call the stored procedure to get all positions
                    var positions = await oracleQuery.GetAllAsync(
                        "position_pkg.get_all_positions",
                        reader => new Position
                        {
                            PositionId = reader.GetInt32(reader.GetOrdinal("POSITIONID")),
                            Title = reader.GetString(reader.GetOrdinal("TITLE")),
                            Description = reader.IsDBNull(reader.GetOrdinal("DESCRIPTION")) ? 
                                null : reader.GetString(reader.GetOrdinal("DESCRIPTION")),
                            Location = reader.IsDBNull(reader.GetOrdinal("LOCATION")) ?
                                null : reader.GetString(reader.GetOrdinal("LOCATION")),
                            Status = reader.IsDBNull(reader.GetOrdinal("STATUS")) ?
                                null : reader.GetString(reader.GetOrdinal("STATUS")),
                            RecruiterId = reader.IsDBNull(reader.GetOrdinal("RECRUITERID")) ?
                                (int?)null : reader.GetInt32(reader.GetOrdinal("RECRUITERID")),
                            DepartmentId = reader.IsDBNull(reader.GetOrdinal("DEPARTMENTID")) ?
                                (int?)null : reader.GetInt32(reader.GetOrdinal("DEPARTMENTID")),
                            Budget = reader.IsDBNull(reader.GetOrdinal("BUDGET")) ?
                                (decimal?)null : reader.GetDecimal(reader.GetOrdinal("BUDGET")),
                            ClosingDate = reader.IsDBNull(reader.GetOrdinal("CLOSINGDATE")) ?
                                (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("CLOSINGDATE"))
                        });

                    // Display the results
                    Console.WriteLine("\nPositions in the database:");
                    Console.WriteLine("--------------------------------------------------------------------------------");
                    Console.WriteLine("ID  | Title                | Location       | Department | Status    | Closing Date  ");
                    Console.WriteLine("--------------------------------------------------------------------------------");
                    
                    foreach (var position in positions)
                    {
                        Console.WriteLine($"{position.PositionId,-3} | {position.Title?.Substring(0, Math.Min(20, position.Title.Length)) + (position.Title?.Length > 20 ? "..." : ""),-20} | " +
                                          $"{position.Location?.Substring(0, Math.Min(13, position.Location.Length)) + (position.Location?.Length > 13 ? "..." : ""),-15} | " +
                                          $"{position.DepartmentId?.ToString() ?? "N/A",-10} | " +
                                          $"{(position.Status ?? "N/A"),-9} | " +
                                          $"{(position.ClosingDate?.ToString("yyyy-MM-dd") ?? "None")}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
