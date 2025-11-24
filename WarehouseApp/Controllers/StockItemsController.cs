using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Data;
using WarehouseApp.Models;

namespace WarehouseApp.Controllers
{
    [Authorize(Roles = "Administrator,Pracownik magazynu,Kierownik magazynu,Audytor")]
    public class StockItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StockItems
        public async Task<IActionResult> Index()
        {
            var items = await _context.StockItems
                .Include(s => s.Product)
                .Include(s => s.WarehouseLocation)   // <-- NAWIGACJA DO LOKALIZACJI
                .ToListAsync();

            return View(items);
        }
    }
}

