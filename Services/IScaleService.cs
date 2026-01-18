using System;
using System.Threading.Tasks;

namespace PontBascule.Services
{
    public interface IScaleService
    {
        Task<bool> ConnectAsync();
        Task DisconnectAsync();
        Task<decimal> ReadWeightAsync();
        bool IsConnected { get; }
        event EventHandler<decimal>? WeightChanged;
    }
}
