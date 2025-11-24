using System.ComponentModel.DataAnnotations;

namespace WarehouseApp.Models
{
    public class BusinessPartner
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        [Display(Name = "Nazwa")]
        public string Name { get; set; }

        [StringLength(20)]
        [Display(Name = "NIP")]
        public string Nip { get; set; }

        [StringLength(300)]
        [Display(Name = "Adres")]
        public string Address { get; set; }

        [Display(Name = "Dostawca")]
        public bool IsSupplier { get; set; }

        [Display(Name = "Odbiorca")]
        public bool IsCustomer { get; set; }
    }
}

