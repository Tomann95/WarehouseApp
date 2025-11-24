using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using WarehouseApp.Data;
using WarehouseApp.Models;
using WarehouseApp.Models.Enums;
using WarehouseApp.Models.Reports;
using WarehouseApp.Services;

namespace WarehouseApp.Controllers
{
    [Authorize(Roles = "Administrator,Kierownik magazynu,Pracownik magazynu")]
    public class WarehouseDocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILocationAllocator _locationAllocator;

        public WarehouseDocumentsController(ApplicationDbContext context,
                                            ILocationAllocator locationAllocator)
        {
            _context = context;
            _locationAllocator = locationAllocator;
        }

        // GET: WarehouseDocuments
        public async Task<IActionResult> Index()
        {
            var documents = await _context.WarehouseDocuments
                .Include(d => d.BusinessPartner)
                .Include(d => d.SourceLocation)
                .Include(d => d.TargetLocation)
                .OrderByDescending(d => d.IssueDate)
                .ThenByDescending(d => d.Id)
                .ToListAsync();

            return View(documents);
        }

        // GET: WarehouseDocuments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var document = await _context.WarehouseDocuments
                .Include(d => d.BusinessPartner)
                .Include(d => d.SourceLocation)
                .Include(d => d.TargetLocation)
                .Include(d => d.Lines)
                    .ThenInclude(l => l.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (document == null)
                return NotFound();

            return View(document);
        }

        // GET: WarehouseDocuments/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            var model = new WarehouseDocument
            {
                IssueDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            return View(model);
        }

        // POST: WarehouseDocuments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WarehouseDocument warehouseDocument)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(warehouseDocument);
                return View(warehouseDocument);
            }

            // ===== AUTOMATYCZNY WYBÓR LOKALIZACJI DLA PZ =====
            if (warehouseDocument.DocumentType == DocumentType.PZ &&
                (warehouseDocument.TargetLocationId == null || warehouseDocument.TargetLocationId == 0))
            {
                // Wczytujemy linie z bazy, jeśli dokument już ma Id
                WarehouseDocument docWithLines = warehouseDocument;

                if (warehouseDocument.Id != 0 && (warehouseDocument.Lines == null || !warehouseDocument.Lines.Any()))
                {
                    docWithLines = await _context.WarehouseDocuments
                        .Include(d => d.Lines)
                        .FirstOrDefaultAsync(d => d.Id == warehouseDocument.Id) ?? warehouseDocument;
                }

                var firstLine = docWithLines.Lines?.FirstOrDefault();
                if (firstLine != null)
                {
                    var bestLocationId =
                        await _locationAllocator.GetBestTargetLocationAsync(firstLine.ProductId);

                    if (bestLocationId.HasValue)
                    {
                        warehouseDocument.TargetLocationId = bestLocationId.Value;
                    }
                }
            }

            if (warehouseDocument.CreatedDate == default)
                warehouseDocument.CreatedDate = DateTime.Now;

            _context.Add(warehouseDocument);
            await _context.SaveChangesAsync();

            // Aktualizacja stanów – jeżeli masz to zrobione w innym miejscu (np. w kontrolerze linii),
            // możesz ten fragment pominąć lub dopasować.
            await ApplyStockChangesAsync(warehouseDocument.Id);

            return RedirectToAction(nameof(Index));
        }

        // GET: WarehouseDocuments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var warehouseDocument = await _context.WarehouseDocuments.FindAsync(id);
            if (warehouseDocument == null)
                return NotFound();

            PopulateDropdowns(warehouseDocument);
            return View(warehouseDocument);
        }

        // POST: WarehouseDocuments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WarehouseDocument warehouseDocument)
        {
            if (id != warehouseDocument.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(warehouseDocument);
                return View(warehouseDocument);
            }

            try
            {
                _context.Update(warehouseDocument);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WarehouseDocumentExists(warehouseDocument.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: WarehouseDocuments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var document = await _context.WarehouseDocuments
                .Include(d => d.BusinessPartner)
                .Include(d => d.SourceLocation)
                .Include(d => d.TargetLocation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (document == null)
                return NotFound();

            return View(document);
        }

        // POST: WarehouseDocuments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _context.WarehouseDocuments
                .Include(d => d.Lines)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document != null)
            {
                // Uproszczenie: nie cofamy stanów przy usuwaniu dokumentu
                _context.WarehouseDocuments.Remove(document);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ===== GENEROWANIE PDF =====
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var document = await _context.WarehouseDocuments
                .Include(d => d.BusinessPartner)
                .Include(d => d.SourceLocation)
                .Include(d => d.TargetLocation)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
                return NotFound();

            var lines = await _context.WarehouseDocumentLines
                .Include(l => l.Product)
                .Where(l => l.WarehouseDocumentId == id)
                .OrderBy(l => l.LineNumber)
                .ToListAsync();

            var report = new WarehouseDocumentPdf(document, lines);
            var pdfBytes = report.GeneratePdf();

            var fileName = $"Dokument_{document.DocumentNumber}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // ===== POMOCNICZE =====

        private void PopulateDropdowns(WarehouseDocument? doc = null)
        {
            ViewData["BusinessPartnerId"] = new SelectList(
                _context.BusinessPartners.OrderBy(b => b.Name),
                "Id",
                "Name",
                doc?.BusinessPartnerId);

            ViewData["SourceLocationId"] = new SelectList(
                _context.WarehouseLocations.OrderBy(l => l.Code),
                "Id",
                "Code",
                doc?.SourceLocationId);

            ViewData["TargetLocationId"] = new SelectList(
                _context.WarehouseLocations.OrderBy(l => l.Code),
                "Id",
                "Code",
                doc?.TargetLocationId);
        }

        private bool WarehouseDocumentExists(int id)
        {
            return _context.WarehouseDocuments.Any(e => e.Id == id);
        }

        /// <summary>
        /// Prosta aktualizacja stanów na podstawie dokumentu.
        /// Jeśli masz już własną logikę w innym miejscu, można ten fragment uprościć
        /// lub całkiem wyłączyć.
        /// </summary>
        private async Task ApplyStockChangesAsync(int documentId)
        {
            var document = await _context.WarehouseDocuments
                .Include(d => d.Lines)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null || document.Lines == null || !document.Lines.Any())
                return;

            foreach (var line in document.Lines)
            {
                switch (document.DocumentType)
                {
                    case DocumentType.PZ:
                        await ChangeStockAsync(line.ProductId, document.TargetLocationId, line.Quantity);
                        break;

                    case DocumentType.WZ:
                        await ChangeStockAsync(line.ProductId, document.SourceLocationId, -line.Quantity);
                        break;

                    case DocumentType.MM:
                        await ChangeStockAsync(line.ProductId, document.SourceLocationId, -line.Quantity);
                        await ChangeStockAsync(line.ProductId, document.TargetLocationId, line.Quantity);
                        break;

                        // PW / RW możesz dopisać analogicznie
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task ChangeStockAsync(int productId, int? locationId, decimal quantityChange)
        {
            if (locationId == null || locationId == 0 || quantityChange == 0)
                return;

            var item = await _context.StockItems
                .FirstOrDefaultAsync(s => s.ProductId == productId &&
                                          s.WarehouseLocationId == locationId.Value);

            if (item == null)
            {
                if (quantityChange > 0)
                {
                    item = new StockItem
                    {
                        ProductId = productId,
                        WarehouseLocationId = locationId.Value,
                        Quantity = quantityChange
                    };
                    _context.StockItems.Add(item);
                }
            }
            else
            {
                item.Quantity += quantityChange;
                if (item.Quantity <= 0)
                {
                    _context.StockItems.Remove(item);
                }
                else
                {
                    _context.StockItems.Update(item);
                }
            }
        }
    }
}

