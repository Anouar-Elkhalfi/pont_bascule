using PontBascule.Models;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Threading.Tasks;

namespace PontBascule.Services
{
    /// <summary>
    /// Service d'impression de tickets
    /// Équivalent Rails: app/services/print_service.rb avec Prawn gem
    /// </summary>
    public class PrintService : IPrintService
    {
        private Weighing? _currentWeighing;

        public Task PrintTicketAsync(Weighing weighing)
        {
            _currentWeighing = weighing;

            var printDocument = new PrintDocument();
            printDocument.PrintPage += PrintPage;

            try
            {
                // Lance l'impression
                // Rails: Prawn::Document.generate("ticket.pdf")
                printDocument.Print();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erreur d'impression: {ex.Message}", ex);
            }
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            if (_currentWeighing == null || e.Graphics == null) return;

            var graphics = e.Graphics;
            var yPos = 50;
            var leftMargin = 50;

            // Polices
            var titleFont = new Font("Arial", 16, FontStyle.Bold);
            var headerFont = new Font("Arial", 12, FontStyle.Bold);
            var textFont = new Font("Arial", 10);

            // En-tête
            graphics.DrawString("TICKET DE PESÉE", titleFont, Brushes.Black, leftMargin, yPos);
            yPos += 40;

            graphics.DrawString("=" + new string('=', 50), textFont, Brushes.Black, leftMargin, yPos);
            yPos += 30;

            // Informations
            DrawLine(graphics, "Date/Heure:", _currentWeighing.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"), 
                    leftMargin, ref yPos, headerFont, textFont);
            DrawLine(graphics, "N° Camion:", _currentWeighing.TruckNumber, 
                    leftMargin, ref yPos, headerFont, textFont);
            DrawLine(graphics, "Transporteur:", _currentWeighing.Transporter, 
                    leftMargin, ref yPos, headerFont, textFont);
            DrawLine(graphics, "Produit:", _currentWeighing.Product, 
                    leftMargin, ref yPos, headerFont, textFont);

            yPos += 20;
            graphics.DrawString("=" + new string('=', 50), textFont, Brushes.Black, leftMargin, yPos);
            yPos += 30;

            // Poids en gros
            graphics.DrawString("POIDS:", headerFont, Brushes.Black, leftMargin, yPos);
            yPos += 30;
            var weightFont = new Font("Arial", 20, FontStyle.Bold);
            graphics.DrawString($"{_currentWeighing.Weight:N0} kg", weightFont, Brushes.Black, 
                              leftMargin + 20, yPos);
            yPos += 40;

            graphics.DrawString($"Type: {_currentWeighing.WeighingType}", headerFont, Brushes.Black, 
                              leftMargin, yPos);
            yPos += 30;

            graphics.DrawString("=" + new string('=', 50), textFont, Brushes.Black, leftMargin, yPos);
            yPos += 30;

            // Footer
            if (!string.IsNullOrEmpty(_currentWeighing.SapDocumentNumber))
            {
                DrawLine(graphics, "Document SAP:", _currentWeighing.SapDocumentNumber, 
                        leftMargin, ref yPos, textFont, textFont);
            }

            yPos += 20;
            graphics.DrawString("Signature: _________________", textFont, Brushes.Black, 
                              leftMargin, yPos);
        }

        private void DrawLine(Graphics graphics, string label, string value, 
                            int leftMargin, ref int yPos, Font labelFont, Font valueFont)
        {
            graphics.DrawString(label, labelFont, Brushes.Black, leftMargin, yPos);
            graphics.DrawString(value, valueFont, Brushes.Black, leftMargin + 150, yPos);
            yPos += 25;
        }

        public async Task<string> GeneratePdfAsync(Weighing weighing)
        {
            // TODO: Implémenter génération PDF avec iTextSharp ou QuestPDF
            // Rails équivalent: Prawn::Document.generate
            
            var pdfPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "PontBascule",
                "Tickets",
                $"Ticket_{weighing.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf"
            );

            Directory.CreateDirectory(Path.GetDirectoryName(pdfPath)!);

            // Simulation pour l'instant
            await File.WriteAllTextAsync(pdfPath, 
                $"Ticket de pesée\nCamion: {weighing.TruckNumber}\nPoids: {weighing.Weight} kg");

            return pdfPath;
        }
    }
}
