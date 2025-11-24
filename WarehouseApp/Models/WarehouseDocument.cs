using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WarehouseApp.Models.Enums;

namespace WarehouseApp.Models
{
    public class WarehouseDocument
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Numer dokumentu")]
        public string DocumentNumber { get; set; }

        [Required]
        [Display(Name = "Typ dokumentu")]
        public DocumentType DocumentType { get; set; }

        [Display(Name = "Data wystawienia")]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Display(Name = "Data utworzenia")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Utworzył")]
        public string CreatedById { get; set; }

        [Display(Name = "Lokalizacja źródłowa")]
        public int? SourceLocationId { get; set; }

        [Display(Name = "Lokalizacja docelowa")]
        public int? TargetLocationId { get; set; }

        [Display(Name = "Kontrahent")]
        public int? BusinessPartnerId { get; set; }

        [Display(Name = "Numer faktury")]
        public string InvoiceNumber { get; set; }

        [Display(Name = "Uwagi")]
        public string Notes { get; set; }

        // Nawigacje
        public ApplicationUser CreatedBy { get; set; }
        public WarehouseLocation SourceLocation { get; set; }
        public WarehouseLocation TargetLocation { get; set; }
        public BusinessPartner BusinessPartner { get; set; }

        public ICollection<WarehouseDocumentLine> Lines { get; set; }
    }
}

