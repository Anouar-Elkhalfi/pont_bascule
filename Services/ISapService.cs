using PontBascule.Models;
using System.Threading.Tasks;

namespace PontBascule.Services
{
    public interface ISapService
    {
        Task<bool> ConnectAsync();
        Task DisconnectAsync();
        Task<string> SendWeighingAsync(Weighing weighing);
        bool IsConnected { get; }
    }
}
