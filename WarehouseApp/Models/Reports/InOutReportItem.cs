namespace WarehouseApp.Models.Reports
{
    public class InOutReportItem
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        public decimal QuantityIn { get; set; }
        public decimal QuantityOut { get; set; }
    }
}






