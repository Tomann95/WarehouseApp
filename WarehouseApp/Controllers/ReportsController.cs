using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Data;
using WarehouseApp.Models.Enums;
using WarehouseApp.Models.Reports;




namespace WarehouseApp.Controllers
{
    [Authorize(Roles = "Administrator,Kierownik magazynu,Pracownik magazynu,Audytor")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ====================================================================
        // 1) RAPORT RUCHÓW MAGAZYNOWYCH (MovementReport)
        //    adres: /Reports/MovementReport
        // ====================================================================

        public async Task<IActionResult> MovementReport(
     DateTime? fromDate,
     DateTime? toDate,
     int? productId,
     int? locationId)
        {
            // Domyślny zakres – ostatnie 30 dni
            var fromDateFilter = fromDate ?? DateTime.Today.AddDays(-30);
            var toDateFilter = toDate ?? DateTime.Today;

            ViewBag.FromDate = fromDateFilter;
            ViewBag.ToDate = toDateFilter;
            ViewBag.ProductId = productId;
            ViewBag.LocationId = locationId;

            // zapytanie bazowe: dokument + linie + towary + lokalizacje + kontrahent
            var query =
                from d in _context.WarehouseDocuments
                join l in _context.WarehouseDocumentLines on d.Id equals l.WarehouseDocumentId
                join p in _context.Products on l.ProductId equals p.Id
                join bp in _context.BusinessPartners on d.BusinessPartnerId equals bp.Id into bpj
                from bp in bpj.DefaultIfEmpty()
                join sl in _context.WarehouseLocations on d.SourceLocationId equals sl.Id into slj
                from sl in slj.DefaultIfEmpty()
                join tl in _context.WarehouseLocations on d.TargetLocationId equals tl.Id into tlj
                from tl in tlj.DefaultIfEmpty()
                where d.IssueDate >= fromDateFilter && d.IssueDate <= toDateFilter
                select new MovementReportItem
                {
                    DocumentNumber = d.DocumentNumber,
                    DocumentType = d.DocumentType,
                    IssueDate = d.IssueDate,
                    BusinessPartnerName = bp != null ? bp.Name : string.Empty,

                    ProductId = p.Id,
                    ProductCode = p.Code,
                    ProductName = p.Name,

                    SourceLocationId = sl != null ? sl.Id : (int?)null,
                    SourceLocationCode = sl != null ? sl.Code : string.Empty,

                    TargetLocationId = tl != null ? tl.Id : (int?)null,
                    TargetLocationCode = tl != null ? tl.Code : string.Empty,

                    Quantity = l.Quantity
                };

            if (productId.HasValue)
            {
                query = query.Where(x => x.ProductId == productId.Value);
            }

            if (locationId.HasValue)
            {
                query = query.Where(x =>
                    x.SourceLocationId == locationId.Value ||
                    x.TargetLocationId == locationId.Value);
            }

            var result = await query
                .OrderBy(x => x.IssueDate)
                .ThenBy(x => x.DocumentNumber)
                .ToListAsync();

            return View(result);
        }


        // ====================================================================
        // 2) RAPORT PRZYJĘĆ / WYDAŃ (InOutReport)
        //    adres: /Reports/InOutReport
        // ====================================================================

        public async Task<IActionResult> InOutReport(
     DateTime? fromDate,
     DateTime? toDate,
     int? productId)
        {
            // NIE używamy nazw "from" / "to", żeby nie gryzły się ze składnią LINQ
            var fromDateFilter = fromDate ?? DateTime.Today.AddDays(-30);
            var toDateFilter = toDate ?? DateTime.Today;

            ViewBag.FromDate = fromDateFilter;
            ViewBag.ToDate = toDateFilter;
            ViewBag.ProductId = productId;

            var lines =
                from d in _context.WarehouseDocuments
                join l in _context.WarehouseDocumentLines on d.Id equals l.WarehouseDocumentId
                join p in _context.Products on l.ProductId equals p.Id
                where d.IssueDate >= fromDateFilter && d.IssueDate <= toDateFilter
                select new
                {
                    d.DocumentType,
                    l.Quantity,
                    ProductId = p.Id,
                    ProductCode = p.Code,
                    ProductName = p.Name
                };

            if (productId.HasValue)
            {
                lines = lines.Where(x => x.ProductId == productId.Value);
            }

            var grouped = await lines
                .GroupBy(x => new { x.ProductId, x.ProductCode, x.ProductName })
                .Select(g => new InOutReportItem
                {
                    ProductId = g.Key.ProductId,
                    ProductCode = g.Key.ProductCode,
                    ProductName = g.Key.ProductName,
                    QuantityIn = g.Where(x =>
                                        x.DocumentType == DocumentType.PZ ||
                                        x.DocumentType == DocumentType.PW)
                                   .Sum(x => x.Quantity),
                    QuantityOut = g.Where(x =>
                                        x.DocumentType == DocumentType.WZ ||
                                        x.DocumentType == DocumentType.RW)
                                   .Sum(x => x.Quantity)
                })
                .OrderBy(x => x.ProductCode)
                .ToListAsync();

            return View(grouped);
        }
        // ====================================================================
        // 3) RAPORT DOKUMENTÓW MAGAZYNOWYCH
        //    adres: /Reports/WarehouseDocument
        // ====================================================================

        public async Task<IActionResult> WarehouseDocument(
            DateTime? fromDate,
            DateTime? toDate,
            DocumentType? documentType,
            int? businessPartnerId)
        {
            var from = fromDate ?? DateTime.Today.AddDays(-30);
            var to = toDate ?? DateTime.Today;

            ViewBag.FromDate = from;
            ViewBag.ToDate = to;
            ViewBag.DocumentType = documentType;
            ViewBag.BusinessPartnerId = businessPartnerId;

            var query = _context.WarehouseDocuments
                .Include(d => d.BusinessPartner)
                .Include(d => d.SourceLocation)
                .Include(d => d.TargetLocation)
                .Where(d => d.IssueDate >= from && d.IssueDate <= to);

            if (documentType.HasValue)
            {
                query = query.Where(d => d.DocumentType == documentType.Value);
            }

            if (businessPartnerId.HasValue)
            {
                query = query.Where(d => d.BusinessPartnerId == businessPartnerId.Value);
            }

            var result = await query
                .OrderByDescending(d => d.IssueDate)
                .ThenBy(d => d.DocumentNumber)
                .ToListAsync();

            return View(result);
        }

    }
}








