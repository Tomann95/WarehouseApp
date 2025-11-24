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
    public class BusinessPartnersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BusinessPartnersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BusinessPartners
        public async Task<IActionResult> Index()
        {
            return View(await _context.BusinessPartners.ToListAsync());
        }

        // GET: BusinessPartners/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var partner = await _context.BusinessPartners
                .FirstOrDefaultAsync(m => m.Id == id);

            if (partner == null) return NotFound();

            return View(partner);
        }

        // GET: BusinessPartners/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BusinessPartners/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BusinessPartner businessPartner)
        {
            if (ModelState.IsValid)
            {
                _context.Add(businessPartner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(businessPartner);
        }

        // GET: BusinessPartners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var partner = await _context.BusinessPartners.FindAsync(id);
            if (partner == null) return NotFound();

            return View(partner);
        }

        // POST: BusinessPartners/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BusinessPartner businessPartner)
        {
            if (id != businessPartner.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(businessPartner);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.BusinessPartners.Any(e => e.Id == businessPartner.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(businessPartner);
        }

        // GET: BusinessPartners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var partner = await _context.BusinessPartners
                .FirstOrDefaultAsync(m => m.Id == id);

            if (partner == null) return NotFound();

            return View(partner);
        }

        // POST: BusinessPartners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var partner = await _context.BusinessPartners.FindAsync(id);

            if (partner != null)
            {
                _context.BusinessPartners.Remove(partner);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
