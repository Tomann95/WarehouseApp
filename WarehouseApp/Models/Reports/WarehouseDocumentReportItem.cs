namespace WarehouseApp.Models.Reports
{
    public class WarehouseDocumentReportItem
    {
        public string DocumentNumber { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }

        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public string SourceLocationCode { get; set; } = string.Empty;
        public string TargetLocationCode { get; set; } = string.Empty;

        public string BusinessPartnerName { get; set; } = string.Empty;
    }
}
