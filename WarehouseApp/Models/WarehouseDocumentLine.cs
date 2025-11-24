using System.ComponentModel.DataAnnotations;

namespace WarehouseApp.Models
{
    public class WarehouseDocumentLine
    {
        public int Id { get; set; }

        [Display(Name = "Dokument")]
        public int WarehouseDocumentId { get; set; }

        [Display(Name = "Towar")]
        public int ProductId { get; set; }

        [Display(Name = "Ilość")]
        public decimal Quantity { get; set; }

        [Display(Name = "Cena jednostkowa")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Lp.")]
        public int LineNumber { get; set; }

        // Nawigacje
        public WarehouseDocument WarehouseDocument { get; set; }
        public Product Product { get; set; }
    }
}
