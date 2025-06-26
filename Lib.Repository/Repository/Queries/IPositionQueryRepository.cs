using Lib.Repository.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lib.Repository.Repository.Queries
{
    public interface IPositionQueryRepository
    {
        Task<IEnumerable<Position>> GetAllPositionsAsync();
        Task<Position?> GetPositionByIdAsync(int positionId);
    }
}
