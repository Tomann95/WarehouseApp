using System.Collections.Generic;

namespace WarehouseApp.Models.ViewModels
{
    public class UserWithRolesViewModel
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? FullName { get; set; }
        public string Roles { get; set; } = string.Empty;
    }

    public class EditUserRolesViewModel
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? FullName { get; set; }

        // checkboxy dla ról
        public bool IsAdministrator { get; set; }
        public bool IsWarehouseWorker { get; set; }      // Pracownik magazynu
        public bool IsWarehouseManager { get; set; }     // Kierownik magazynu
        public bool IsAuditor { get; set; }              // Audytor
    }
}

