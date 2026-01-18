using PontBascule.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PontBascule.Services
{
    public interface IDatabaseService
    {
        Task InitializeDatabaseAsync();
        Task<int> SaveWeighingAsync(Weighing weighing);
        Task<Weighing?> GetWeighingByIdAsync(int id);
        Task<List<Weighing>> GetRecentWeighingsAsync(int count = 50);
        Task UpdateWeighingAsync(Weighing weighing);
    }
}
