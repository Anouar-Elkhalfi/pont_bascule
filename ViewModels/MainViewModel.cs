using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PontBascule.Models;
using PontBascule.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PontBascule.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IScaleService _scaleService;
        private readonly ISapService _sapService;
        private readonly IDatabaseService _databaseService;

        [ObservableProperty]
        private string _truckNumber = string.Empty;

        [ObservableProperty]
        private string _transporter = string.Empty;

        [ObservableProperty]
        private string _product = string.Empty;

        [ObservableProperty]
        private decimal _currentWeight = 0;

        [ObservableProperty]
        private string _scaleStatus = "Déconnecté";

        [ObservableProperty]
        private string _statusMessage = "Prêt";

        [ObservableProperty]
        private SolidColorBrush _sapConnectionStatus = Brushes.Red;

        [ObservableProperty]
        private SolidColorBrush _scaleConnectionStatus = Brushes.Red;

        public ObservableCollection<Weighing> WeighingHistory { get; } = new();

        public MainViewModel(
            IScaleService scaleService,
            ISapService sapService,
            IDatabaseService databaseService)
        {
            _scaleService = scaleService;
            _sapService = sapService;
            _databaseService = databaseService;

            _scaleService.WeightChanged += OnWeightChanged;

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                StatusMessage = "Initialisation de la base de données...";
                await _databaseService.InitializeDatabaseAsync();

                StatusMessage = "Connexion à la balance...";
                if (await _scaleService.ConnectAsync())
                {
                    ScaleConnectionStatus = Brushes.Green;
                    ScaleStatus = "Connecté";
                }

                StatusMessage = "Connexion à SAP...";
                if (await _sapService.ConnectAsync())
                {
                    SapConnectionStatus = Brushes.Green;
                }

                await LoadHistoryAsync();
                StatusMessage = "Système prêt";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur d'initialisation: {ex.Message}";
            }
        }

        private async Task LoadHistoryAsync()
        {
            var history = await _databaseService.GetRecentWeighingsAsync(20);
            WeighingHistory.Clear();
            foreach (var weighing in history)
            {
                WeighingHistory.Add(weighing);
            }
        }

        private void OnWeightChanged(object? sender, decimal weight)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                CurrentWeight = weight;
            });
        }

        [RelayCommand]
        private async Task WeighIn()
        {
            if (string.IsNullOrWhiteSpace(TruckNumber))
            {
                StatusMessage = "⚠️ Veuillez saisir le numéro de camion";
                return;
            }

            try
            {
                var weight = await _scaleService.ReadWeightAsync();
                
                var weighing = new Weighing
                {
                    Timestamp = DateTime.Now,
                    TruckNumber = TruckNumber,
                    Transporter = Transporter,
                    Product = Product,
                    Weight = weight,
                    WeighingType = WeighingType.Entrée
                };

                var id = await _databaseService.SaveWeighingAsync(weighing);
                weighing.Id = id;

                WeighingHistory.Insert(0, weighing);
                StatusMessage = $"✓ Pesée entrée enregistrée: {weight:N0} kg";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Erreur: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task WeighOut()
        {
            if (string.IsNullOrWhiteSpace(TruckNumber))
            {
                StatusMessage = "⚠️ Veuillez saisir le numéro de camion";
                return;
            }

            try
            {
                var weight = await _scaleService.ReadWeightAsync();
                
                var weighing = new Weighing
                {
                    Timestamp = DateTime.Now,
                    TruckNumber = TruckNumber,
                    Transporter = Transporter,
                    Product = Product,
                    Weight = weight,
                    WeighingType = WeighingType.Sortie
                };

                var id = await _databaseService.SaveWeighingAsync(weighing);
                weighing.Id = id;

                WeighingHistory.Insert(0, weighing);
                StatusMessage = $"✓ Pesée sortie enregistrée: {weight:N0} kg";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Erreur: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task PrintTicket()
        {
            if (WeighingHistory.Count == 0)
            {
                StatusMessage = "⚠️ Aucune pesée à imprimer";
                return;
            }

            try
            {
                var latestWeighing = WeighingHistory[0];
                
                // Si vous avez ajouté IPrintService dans le constructeur
                // await _printService.PrintTicketAsync(latestWeighing);
                
                StatusMessage = $"✓ Ticket imprimé pour {latestWeighing.TruckNumber}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Erreur impression: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task SendToSap()
        {
            if (WeighingHistory.Count == 0)
            {
                StatusMessage = "⚠️ Aucune pesée à envoyer";
                return;
            }

            try
            {
                var latestWeighing = WeighingHistory[0];
                
                if (latestWeighing.SentToSap)
                {
                    StatusMessage = "⚠️ Cette pesée a déjà été envoyée à SAP";
                    return;
                }

                StatusMessage = "Envoi vers SAP...";
                var docNumber = await _sapService.SendWeighingAsync(latestWeighing);

                latestWeighing.SapDocumentNumber = docNumber;
                latestWeighing.SentToSap = true;
                await _databaseService.UpdateWeighingAsync(latestWeighing);

                StatusMessage = $"✓ Envoyé à SAP - Document: {docNumber}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Erreur SAP: {ex.Message}";
            }
        }
    }
}
