using System.ComponentModel.DataAnnotations;

namespace WarehouseApp.Models
{
    public class StockItem
    {
        public int Id { get; set; }

        [Display(Name = "Towar")]
        public int ProductId { get; set; }

        [Display(Name = "Lokalizacja")]
        public int WarehouseLocationId { get; set; }

        [Display(Name = "Ilość")]
        public decimal Quantity { get; set; }

        // Nawigacje
        public Product Product { get; set; }
        public WarehouseLocation WarehouseLocation { get; set; }
    }
}


