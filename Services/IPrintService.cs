using PontBascule.Models;
using System.Threading.Tasks;

namespace PontBascule.Services
{
    /// <summary>
    /// Service d'impression de tickets de pesée
    /// Parallèle Rails: app/services/print_service.rb
    /// </summary>
    public interface IPrintService
    {
        /// <summary>
        /// Imprime un ticket de pesée
        /// Rails: def print_ticket(weighing)
        /// </summary>
        Task PrintTicketAsync(Weighing weighing);
        
        /// <summary>
        /// Génère un PDF du ticket
        /// Rails: def generate_pdf(weighing)
        /// </summary>
        Task<string> GeneratePdfAsync(Weighing weighing);
    }
}
