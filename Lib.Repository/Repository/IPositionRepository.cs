
using Lib.Repository.Entities;
namespace Lib.Repository.Repository {
    public interface IPositionRepository
    {
        Task<IEnumerable<Position>> GetAllPositionsAsync();
        Task<Position?> GetPositionByIdAsync(int positionId);
        Task<int?> AddPositionAsync(Position position);
        Task<bool> UpdatePositionAsync(Position position);
        Task<bool> RemovePositionAsync(int positionId);
    }
}
