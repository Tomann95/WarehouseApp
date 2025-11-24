using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Data;

namespace WarehouseApp.Services
{
    /// <summary>
    /// Prosta implementacja wyboru lokalizacji:
    /// - szuka lokalizacji, dla której NIE ma stanu dla danego produktu (pusta dla tego towaru),
    /// - jeśli nie znajdzie, zwraca pierwszą lokalizację po kodzie.
    /// </summary>
    public class SimpleLocationAllocator : ILocationAllocator
    {
        private readonly ApplicationDbContext _context;

        public SimpleLocationAllocator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetBestTargetLocationAsync(int productId)
        {
            // 1) lokalizacje, w których nie ma jeszcze stanu dla tego produktu
            var emptyLocation = await _context.WarehouseLocations
                .Where(loc =>
                    !_context.StockItems.Any(si =>
                        si.ProductId == productId &&
                        si.WarehouseLocationId == loc.Id))
                .OrderBy(loc => loc.Code)
                .FirstOrDefaultAsync();

            if (emptyLocation != null)
                return emptyLocation.Id;

            // 2) fallback – pierwsza lokalizacja po kodzie
            var anyLocation = await _context.WarehouseLocations
                .OrderBy(loc => loc.Code)
                .FirstOrDefaultAsync();

            return anyLocation?.Id;
        }
    }
}

