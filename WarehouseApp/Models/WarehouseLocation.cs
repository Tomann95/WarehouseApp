using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WarehouseApp.Models
{
    public class WarehouseLocation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Kod lokalizacji")]
        public string Code { get; set; }

        [StringLength(200)]
        [Display(Name = "Opis")]
        public string Description { get; set; }

        [Display(Name = "Aktywna")]
        public bool IsActive { get; set; } = true;

        public ICollection<StockItem> StockItems { get; set; }
    }
}



