using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WarehouseApp.Models;

namespace WarehouseApp.Models.Reports
{
    public class WarehouseDocumentPdf : IDocument
    {
        private readonly WarehouseDocument _document;
        private readonly IList<WarehouseDocumentLine> _lines;

        public WarehouseDocumentPdf(WarehouseDocument document, IList<WarehouseDocumentLine> lines)
        {
            _document = document;
            _lines = lines ?? new List<WarehouseDocumentLine>();
        }

        // statyczna metoda wygodna dla kontrolera
        public static byte[] Generate(WarehouseDocument document, IList<WarehouseDocumentLine> lines)
        {
            var doc = new WarehouseDocumentPdf(document, lines);
            return doc.GeneratePdf();   // rozszerzenie z QuestPDF.Fluent
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            var lineModels = _lines
                .OrderBy(l => l.LineNumber)
                .Select(l => new
                {
                    l.LineNumber,
                    ProductName = l.Product?.Name ?? string.Empty,
                    ProductCode = l.Product?.Code ?? string.Empty,
                    l.Quantity
                })
                .ToList();

            var totalQuantity = lineModels.Sum(x => x.Quantity);

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);

                // NAGŁÓWEK
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Dokument magazynowy").FontSize(20).SemiBold();
                        col.Item().Text($"Numer: {_document.DocumentNumber}").FontSize(12);
                        col.Item().Text($"Typ: {_document.DocumentType}").FontSize(12);
                        col.Item().Text($"Data wystawienia: {_document.IssueDate:yyyy-MM-dd}").FontSize(12);

                        if (!string.IsNullOrWhiteSpace(_document.InvoiceNumber))
                            col.Item().Text($"Numer faktury: {_document.InvoiceNumber}").FontSize(12);
                    });

                    row.ConstantItem(150).Column(col =>
                    {
                        col.Item().AlignRight().Text("WarehouseApp").FontSize(12).SemiBold();
                        col.Item().AlignRight().Text("Autor: Mateusz Tomanek").FontSize(10);
                        col.Item().AlignRight().Text($"Wygenerowano: {DateTime.Now:yyyy-MM-dd HH:mm}")
                            .FontSize(9);
                    });
                });

                // TREŚĆ
                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    // KONTRAHENT
                    col.Item().Column(c =>
                    {
                        c.Spacing(2);
                        c.Item().Text("Kontrahent").SemiBold().FontSize(12);

                        if (_document.BusinessPartner != null)
                        {
                            c.Item().Text(_document.BusinessPartner.Name).FontSize(10);
                            if (!string.IsNullOrWhiteSpace(_document.BusinessPartner.Address))
                                c.Item().Text(_document.BusinessPartner.Address).FontSize(10);
                        }
                        else
                        {
                            c.Item().Text("Brak przypisanego kontrahenta.").FontSize(10);
                        }
                    });

                    // LOKALIZACJE
                    col.Item().Column(c =>
                    {
                        c.Spacing(2);
                        c.Item().Text("Lokalizacje").SemiBold().FontSize(12);

                        var source = _document.SourceLocation?.Code ?? "-";
                        var target = _document.TargetLocation?.Code ?? "-";

                        c.Item().Text($"Źródłowa: {source}").FontSize(10);
                        c.Item().Text($"Docelowa: {target}").FontSize(10);
                    });

                    // TABELA POZYCJI
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);   // LP
                            columns.RelativeColumn(4);    // Produkt
                            columns.RelativeColumn(2);    // Kod
                            columns.RelativeColumn(2);    // Ilość
                        });

                        // nagłówek
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3)
                                .Text("Lp").SemiBold().FontSize(10);
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3)
                                .Text("Produkt").SemiBold().FontSize(10);
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3)
                                .Text("Kod").SemiBold().FontSize(10);
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight()
                                .Text("Ilość").SemiBold().FontSize(10);
                        });

                        foreach (var line in lineModels)
                        {
                            table.Cell().Padding(3).Text(line.LineNumber.ToString()).FontSize(9);
                            table.Cell().Padding(3).Text(line.ProductName).FontSize(9);
                            table.Cell().Padding(3).Text(line.ProductCode).FontSize(9);
                            table.Cell().Padding(3).AlignRight()
                                .Text(line.Quantity.ToString("0.###")).FontSize(9);
                        }

                        // podsumowanie
                        table.Cell().ColumnSpan(3).PaddingTop(5).AlignRight()
                            .Text("Razem ilość:").SemiBold().FontSize(10);
                        table.Cell().PaddingTop(5).AlignRight()
                            .Text(totalQuantity.ToString("0.###")).SemiBold().FontSize(10);
                    });
                });

                // STOPKA
                // STOPKA
                page.Footer()
                    .AlignCenter()
                    .Text($"© {DateTime.Now.Year} - WarehouseApp - Autor: Mateusz Tomanek");

            });
        }
    }
}


