using PontBascule.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PontBascule.Services
{
    /// <summary>
    /// Service d'export de données
    /// Parallèle Rails: app/services/export_service.rb
    /// </summary>
    public class ExportService : IExportService
    {
        private readonly string _exportPath;

        public ExportService()
        {
            _exportPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "PontBascule",
                "Exports"
            );
            Directory.CreateDirectory(_exportPath);
        }

        public async Task<string> ExportToExcelAsync(List<Weighing> weighings, DateTime? startDate = null, DateTime? endDate = null)
        {
            // TODO: Implémenter avec EPPlus
            // Installation: dotnet add package EPPlus
            
            /* Exemple avec EPPlus:
            
            using OfficeOpenXml;
            
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            var fileName = $"Pesees_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(_exportPath, fileName);
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Pesées");
            
            // En-têtes
            worksheet.Cells[1, 1].Value = "Date/Heure";
            worksheet.Cells[1, 2].Value = "N° Camion";
            worksheet.Cells[1, 3].Value = "Transporteur";
            worksheet.Cells[1, 4].Value = "Produit";
            worksheet.Cells[1, 5].Value = "Poids (kg)";
            worksheet.Cells[1, 6].Value = "Type";
            worksheet.Cells[1, 7].Value = "Document SAP";
            
            // Style en-têtes
            using var headerRange = worksheet.Cells[1, 1, 1, 7];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            
            // Données
            int row = 2;
            foreach (var w in weighings)
            {
                worksheet.Cells[row, 1].Value = w.Timestamp.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cells[row, 2].Value = w.TruckNumber;
                worksheet.Cells[row, 3].Value = w.Transporter;
                worksheet.Cells[row, 4].Value = w.Product;
                worksheet.Cells[row, 5].Value = w.Weight;
                worksheet.Cells[row, 6].Value = w.WeighingType.ToString();
                worksheet.Cells[row, 7].Value = w.SapDocumentNumber ?? "";
                row++;
            }
            
            // Totaux
            worksheet.Cells[row, 4].Value = "TOTAL:";
            worksheet.Cells[row, 5].Formula = $"SUM(E2:E{row-1})";
            worksheet.Cells[row, 4, row, 5].Style.Font.Bold = true;
            
            // Auto-fit colonnes
            worksheet.Cells.AutoFitColumns();
            
            await package.SaveAsAsync(new FileInfo(filePath));
            return filePath;
            */

            // Simulation pour l'instant
            var fileName = $"Pesees_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(_exportPath, fileName);
            
            await File.WriteAllTextAsync(filePath, 
                $"Export Excel simulé\nNombre de pesées: {weighings.Count}");
            
            return filePath;
        }

        public async Task<string> ExportToCsvAsync(List<Weighing> weighings)
        {
            var fileName = $"Pesees_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var filePath = Path.Combine(_exportPath, fileName);

            var csv = new StringBuilder();
            
            // En-tête CSV
            csv.AppendLine("Date/Heure;N° Camion;Transporteur;Produit;Poids (kg);Type;Document SAP");
            
            // Données
            foreach (var w in weighings)
            {
                csv.AppendLine($"{w.Timestamp:dd/MM/yyyy HH:mm};" +
                              $"{w.TruckNumber};" +
                              $"{w.Transporter};" +
                              $"{w.Product};" +
                              $"{w.Weight};" +
                              $"{w.WeighingType};" +
                              $"{w.SapDocumentNumber ?? ""}");
            }
            
            // Totaux
            var totalWeight = weighings.Sum(w => w.Weight);
            csv.AppendLine($";;;TOTAL;{totalWeight};;");

            await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
            
            return filePath;
        }

        public async Task<string> GeneratePdfReportAsync(List<Weighing> weighings, DateTime startDate, DateTime endDate)
        {
            // TODO: Implémenter avec QuestPDF
            // Installation: dotnet add package QuestPDF
            
            /* Exemple avec QuestPDF:
            
            using QuestPDF.Fluent;
            using QuestPDF.Helpers;
            using QuestPDF.Infrastructure;
            
            var fileName = $"Rapport_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf";
            var filePath = Path.Combine(_exportPath, fileName);
            
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text($"RAPPORT DE PESÉES - Du {startDate:dd/MM/yyyy} au {endDate:dd/MM/yyyy}")
                        .SemiBold().FontSize(16).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Date
                                columns.RelativeColumn(2); // Camion
                                columns.RelativeColumn(2); // Transporteur
                                columns.RelativeColumn(2); // Produit
                                columns.RelativeColumn(1); // Poids
                                columns.RelativeColumn(1); // Type
                            });

                            // En-tête
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Date/Heure");
                                header.Cell().Element(CellStyle).Text("N° Camion");
                                header.Cell().Element(CellStyle).Text("Transporteur");
                                header.Cell().Element(CellStyle).Text("Produit");
                                header.Cell().Element(CellStyle).Text("Poids (kg)");
                                header.Cell().Element(CellStyle).Text("Type");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .DefaultTextStyle(x => x.SemiBold())
                                        .PaddingVertical(5)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Black);
                                }
                            });

                            // Données
                            foreach (var w in weighings)
                            {
                                table.Cell().Element(CellStyle).Text(w.Timestamp.ToString("dd/MM/yyyy HH:mm"));
                                table.Cell().Element(CellStyle).Text(w.TruckNumber);
                                table.Cell().Element(CellStyle).Text(w.Transporter);
                                table.Cell().Element(CellStyle).Text(w.Product);
                                table.Cell().Element(CellStyle).Text($"{w.Weight:N0}");
                                table.Cell().Element(CellStyle).Text(w.WeighingType.ToString());

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(5);
                                }
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                });
            })
            .GeneratePdf(filePath);
            
            return filePath;
            */

            // Simulation
            var fileName = $"Rapport_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf";
            var filePath = Path.Combine(_exportPath, fileName);
            
            await File.WriteAllTextAsync(filePath, 
                $"Rapport PDF simulé\nPériode: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}\n" +
                $"Nombre de pesées: {weighings.Count}");
            
            return filePath;
        }
    }
}
