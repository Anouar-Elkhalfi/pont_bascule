using System;

namespace PontBascule.Models
{
    public class Weighing
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string TruckNumber { get; set; } = string.Empty;
        public string Transporter { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public WeighingType WeighingType { get; set; }
        public string? SapDocumentNumber { get; set; }
        public bool SentToSap { get; set; }
        public string? Notes { get; set; }
    }

    public enum WeighingType
    {
        Entrée,
        Sortie
    }
}
