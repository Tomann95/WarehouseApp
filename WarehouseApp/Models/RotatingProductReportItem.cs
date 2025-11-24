namespace WarehouseApp.Models
{
    public class RotatingProductReportItem
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;

        // Łączna ilość wydana (WZ) w badanym okresie
        public decimal QuantityOut { get; set; }
    }
}

