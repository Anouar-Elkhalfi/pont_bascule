using System;

namespace PontBascule.Models
{
    /// <summary>
    /// Configuration globale de l'application
    /// Parallèle Rails: Settings ou config variables
    /// </summary>
    public class AppConfiguration
    {
        public string CompanyName { get; set; } = "Votre Entreprise";
        public string SiteName { get; set; } = "Site Principal";
        public string LogoPath { get; set; } = string.Empty;
        public bool AutoPrintTicket { get; set; } = true;
        public bool AutoSendToSap { get; set; } = false;
        public int WeightStabilizationDelay { get; set; } = 2000; // ms
    }

    /// <summary>
    /// Résultat d'une pesée avec calcul net
    /// Parallèle Rails: Decorator pattern ou computed attributes
    /// </summary>
    public class WeighingResult
    {
        public Weighing EntryWeighing { get; set; } = null!;
        public Weighing? ExitWeighing { get; set; }
        
        public decimal NetWeight => 
            ExitWeighing != null 
            ? Math.Abs(ExitWeighing.Weight - EntryWeighing.Weight)
            : 0;
        
        public bool IsComplete => ExitWeighing != null;
        
        public TimeSpan Duration =>
            ExitWeighing != null
            ? ExitWeighing.Timestamp - EntryWeighing.Timestamp
            : TimeSpan.Zero;
    }
}
