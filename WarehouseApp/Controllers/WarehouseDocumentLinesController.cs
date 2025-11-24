using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Data;
using WarehouseApp.Models;
using WarehouseApp.Models.Enums;

namespace WarehouseApp.Controllers
{
    public class WarehouseDocumentLinesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WarehouseDocumentLinesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: WarehouseDocumentLines
        public async Task<IActionResult> Index()
        {
            var lines = _context.WarehouseDocumentLines
                .Include(l => l.WarehouseDocument)
                .Include(l => l.Product);

            return View(await lines.ToListAsync());
        }

        // GET: WarehouseDocumentLines/Create
        // /WarehouseDocumentLines/Create?warehouseDocumentId=5
        public async Task<IActionResult> Create(int warehouseDocumentId)
        {
            var document = await _context.WarehouseDocuments.FindAsync(warehouseDocumentId);
            if (document == null)
            {
                return NotFound();
            }

            ViewData["WarehouseDocumentId"] = warehouseDocumentId;
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");

            // kolejne LP
            var nextLineNumber = await _context.WarehouseDocumentLines
                .Where(l => l.WarehouseDocumentId == warehouseDocumentId)
                .Select(l => (int?)l.LineNumber)
                .MaxAsync() ?? 0;

            var model = new WarehouseDocumentLine
            {
                WarehouseDocumentId = warehouseDocumentId,
                LineNumber = nextLineNumber + 1
            };

            return View(model);
        }

        // POST: WarehouseDocumentLines/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WarehouseDocumentLine line)
        {
            // IGNORUJEMY ModelState – zapisujemy zawsze
            _context.WarehouseDocumentLines.Add(line);

            // aktualizacja stanów – nowa pozycja => +Quantity
            await ApplyStockChangeAsync(line, line.Quantity);

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "WarehouseDocuments", new { id = line.WarehouseDocumentId });
        }

        // GET: WarehouseDocumentLines/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.WarehouseDocumentLines == null)
            {
                return NotFound();
            }

            var line = await _context.WarehouseDocumentLines
                .Include(l => l.WarehouseDocument)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (line == null)
            {
                return NotFound();
            }

            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", line.ProductId);
            return View(line);
        }

        // POST: WarehouseDocumentLines/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WarehouseDocumentLine line)
        {
            if (id != line.Id)
            {
                return NotFound();
            }

            // żadnego ModelState.IsValid – zawsze przeliczamy
            var existingLine = await _context.WarehouseDocumentLines
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == line.Id);

            if (existingLine == null)
            {
                return NotFound();
            }

            decimal quantityDelta = line.Quantity - existingLine.Quantity;

            _context.Update(line);

            if (quantityDelta != 0)
            {
                await ApplyStockChangeAsync(line, quantityDelta);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "WarehouseDocuments", new { id = line.WarehouseDocumentId });
        }

        // GET: WarehouseDocumentLines/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.WarehouseDocumentLines == null)
            {
                return NotFound();
            }

            var line = await _context.WarehouseDocumentLines
                .Include(l => l.WarehouseDocument)
                .Include(l => l.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (line == null)
            {
                return NotFound();
            }

            return View(line);
        }

        // POST: WarehouseDocumentLines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.WarehouseDocumentLines == null)
            {
                return Problem("Entity set 'ApplicationDbContext.WarehouseDocumentLines' is null.");
            }

            var line = await _context.WarehouseDocumentLines.FindAsync(id);
            if (line == null)
            {
                return NotFound();
            }

            int documentId = line.WarehouseDocumentId;

            // usuwamy pozycję => -Quantity
            await ApplyStockChangeAsync(line, -line.Quantity);

            _context.WarehouseDocumentLines.Remove(line);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "WarehouseDocuments", new { id = documentId });
        }

        private bool WarehouseDocumentLineExists(int id)
        {
            return (_context.WarehouseDocumentLines?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // ================== LOGIKA STANÓW MAGAZYNOWYCH ==================

        private async Task ApplyStockChangeAsync(WarehouseDocumentLine line, decimal quantityDelta)
        {
            var document = await _context.WarehouseDocuments.FindAsync(line.WarehouseDocumentId);
            if (document == null)
                return;

            int? sourceLocationId = document.SourceLocationId;
            int? targetLocationId = document.TargetLocationId;
            int productId = line.ProductId;

            decimal sourceChange = 0;
            decimal targetChange = 0;

            switch (document.DocumentType)
            {
                case DocumentType.PZ:
                case DocumentType.PW:
                    // przyjęcie / przychód wewnętrzny -> plus na docelowej
                    targetChange = quantityDelta;
                    break;

                case DocumentType.WZ:
                case DocumentType.RW:
                    // wydanie / rozchód wewnętrzny -> minus na źródłowej
                    sourceChange = -quantityDelta;
                    break;

                case DocumentType.MM:
                    // międzymagazynowy -> minus na źródle, plus na celu
                    sourceChange = -quantityDelta;
                    targetChange = quantityDelta;
                    break;

                default:
                    // inne typy – na razie bez wpływu na stany
                    break;
            }

            if (sourceLocationId.HasValue && sourceChange != 0)
            {
                await UpdateStockItemAsync(productId, sourceLocationId.Value, sourceChange);
            }

            if (targetLocationId.HasValue && targetChange != 0)
            {
                await UpdateStockItemAsync(productId, targetLocationId.Value, targetChange);
            }
        }

        private async Task UpdateStockItemAsync(int productId, int locationId, decimal quantityChange)
        {
            var stockItem = await _context.StockItems
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseLocationId == locationId);

            if (stockItem == null)
            {
                stockItem = new StockItem
                {
                    ProductId = productId,
                    WarehouseLocationId = locationId,
                    Quantity = 0
                };
                _context.StockItems.Add(stockItem);
            }

            stockItem.Quantity += quantityChange;

            // jeśli chcesz, możesz zablokować zejście poniżej zera:
            // if (stockItem.Quantity < 0) stockItem.Quantity = 0;
        }
    }
}



