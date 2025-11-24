using System;
using WarehouseApp.Models.Enums;

namespace WarehouseApp.Models.Reports
{
    public class MovementReportItem
    {
        // Informacje o dokumencie
        public string DocumentNumber { get; set; } = string.Empty;
        public DocumentType DocumentType { get; set; }
        public DateTime IssueDate { get; set; }

        // Kontrahent
        public string BusinessPartnerName { get; set; } = string.Empty;

        // Towar
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        // Lokalizacje
        public int? SourceLocationId { get; set; }
        public string SourceLocationCode { get; set; } = string.Empty;

        public int? TargetLocationId { get; set; }
        public string TargetLocationCode { get; set; } = string.Empty;

        // Ilość
        public decimal Quantity { get; set; }
    }
}





