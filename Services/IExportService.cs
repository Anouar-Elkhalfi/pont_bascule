using PontBascule.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PontBascule.Services
{
    /// <summary>
    /// Service d'export de données
    /// Parallèle Rails: app/services/export_service.rb avec gems csv/excel
    /// </summary>
    public interface IExportService
    {
        /// <summary>
        /// Exporte les pesées vers Excel
        /// Rails: def export_to_excel(weighings)
        /// </summary>
        Task<string> ExportToExcelAsync(List<Weighing> weighings, DateTime? startDate = null, DateTime? endDate = null);
        
        /// <summary>
        /// Exporte les pesées vers CSV
        /// Rails: def export_to_csv(weighings)
        /// </summary>
        Task<string> ExportToCsvAsync(List<Weighing> weighings);
        
        /// <summary>
        /// Génère un rapport PDF
        /// Rails: def generate_pdf_report(weighings)
        /// </summary>
        Task<string> GeneratePdfReportAsync(List<Weighing> weighings, DateTime startDate, DateTime endDate);
    }
}
