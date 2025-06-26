
using Lib.Repository.Entities;    
namespace Lib.Repository.Repository {
    public interface IInitializeRepository
    {
        Task<bool> InitializeDatabaseAsync(string connectionString);
    }
}

