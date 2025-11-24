namespace WarehouseApp.Models.Enums
{
    public enum DocumentType
    {
        PZ = 0, // Przyjęcie zewnętrzne
        WZ = 1, // Wydanie zewnętrzne
        MM = 2, // Przesunięcie międzymagazynowe
        PW = 3, // Przyjęcie wewnętrzne
        RW = 4  // Rozchód wewnętrzny
    }
}

