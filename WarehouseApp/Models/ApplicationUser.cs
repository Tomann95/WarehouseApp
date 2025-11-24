using Microsoft.AspNetCore.Identity;

namespace WarehouseApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Domyślnie pusty string, żeby nigdy nie było NULL
        public string FullName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}


