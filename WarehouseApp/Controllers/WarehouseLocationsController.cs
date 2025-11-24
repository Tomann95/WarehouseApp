using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Data;
using WarehouseApp.Models;

namespace WarehouseApp.Controllers
{
    [Authorize(Roles = "Administrator,Kierownik magazynu")]
    public class WarehouseLocationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WarehouseLocationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: WarehouseLocations
        public async Task<IActionResult> Index()
        {
            if (_context.WarehouseLocations == null)
            {
                return Problem("Entity set 'ApplicationDbContext.WarehouseLocations' is null.");
            }

            var locations = await _context.WarehouseLocations.ToListAsync();
            return View(locations);
        }

        // GET: WarehouseLocations/Create
        public IActionResult Create()
        {
            return View(new WarehouseLocation { IsActive = true });
        }

        // POST: WarehouseLocations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WarehouseLocation warehouseLocation)
        {
            // Zapis BEZ sprawdzania ModelState – żeby na pewno działało
            _context.WarehouseLocations.Add(warehouseLocation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: WarehouseLocations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.WarehouseLocations == null)
            {
                return NotFound();
            }

            var warehouseLocation = await _context.WarehouseLocations.FindAsync(id);
            if (warehouseLocation == null)
            {
                return NotFound();
            }
            return View(warehouseLocation);
        }

        // POST: WarehouseLocations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WarehouseLocation warehouseLocation)
        {
            if (id != warehouseLocation.Id)
            {
                return NotFound();
            }

            _context.Update(warehouseLocation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool WarehouseLocationExists(int id)
        {
            return (_context.WarehouseLocations?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
