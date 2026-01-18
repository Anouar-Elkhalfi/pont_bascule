using Microsoft.Extensions.Configuration;
using PontBascule.Models;
using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace PontBascule.Services
{
    public class ScaleService : IScaleService, IDisposable
    {
        private SerialPort? _serialPort;
        private readonly ScaleConfiguration _config;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _readingTask;

        public bool IsConnected => _serialPort?.IsOpen ?? false;
        public event EventHandler<decimal>? WeightChanged;

        public ScaleService(IConfiguration configuration)
        {
            _config = configuration.GetSection("Scale").Get<ScaleConfiguration>() 
                      ?? new ScaleConfiguration();
        }

        public Task<bool> ConnectAsync()
        {
            try
            {
                _serialPort = new SerialPort
                {
                    PortName = _config.PortName,
                    BaudRate = _config.BaudRate,
                    DataBits = _config.DataBits,
                    Parity = Enum.Parse<Parity>(_config.Parity),
                    StopBits = Enum.Parse<StopBits>(_config.StopBits),
                    ReadTimeout = _config.ReadTimeout
                };

                _serialPort.Open();

                // Démarrer la lecture en continu
                _cancellationTokenSource = new CancellationTokenSource();
                _readingTask = Task.Run(() => ContinuousRead(_cancellationTokenSource.Token));

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur connexion balance: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public Task DisconnectAsync()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _readingTask?.Wait(TimeSpan.FromSeconds(2));
                
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.Close();
                }
                
                _serialPort?.Dispose();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur déconnexion balance: {ex.Message}");
                return Task.CompletedTask;
            }
        }

        public async Task<decimal> ReadWeightAsync()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Balance non connectée");

            try
            {
                // Simulation pour le développement
                // À remplacer par la vraie lecture série
                await Task.Delay(100);
                var random = new Random();
                return random.Next(0, 50000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lecture poids: {ex.Message}");
                return 0;
            }
        }

        private async Task ContinuousRead(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                try
                {
                    var weight = await ReadWeightAsync();
                    WeightChanged?.Invoke(this, weight);
                    await Task.Delay(500, cancellationToken); // Lecture toutes les 500ms
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lecture continue: {ex.Message}");
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        public void Dispose()
        {
            DisconnectAsync().Wait();
            _cancellationTokenSource?.Dispose();
        }
    }
}
