using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WarehouseApp.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Nazwa")]
        public string Name { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Kod")]
        public string Code { get; set; }

        [StringLength(50)]
        [Display(Name = "Kategoria")]
        public string Category { get; set; }

        [Required, StringLength(10)]
        [Display(Name = "Jednostka")]
        public string Unit { get; set; }

        [Display(Name = "Cena")]
        [DataType(DataType.Currency)]
        public decimal? Price { get; set; }

        [Display(Name = "Opis")]
        public string Description { get; set; }

        [Display(Name = "Aktywny")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Kod kreskowy / QR")]
        public string? Barcode { get; set; }


        public ICollection<StockItem> StockItems { get; set; }
    }
}

