using Lib.Repository.Entities;
using System.Threading.Tasks;

namespace Lib.Repository.Repository.Commands
{
    public interface IPositionCommandRepository
    {
        Task<int?> AddPositionAsync(Position position);
        Task<bool> UpdatePositionAsync(Position position);
        Task<bool> RemovePositionAsync(int positionId);
    }
}
