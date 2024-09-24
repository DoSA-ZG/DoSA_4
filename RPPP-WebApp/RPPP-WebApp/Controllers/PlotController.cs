using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using PagedList;

namespace RPPP_WebApp.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PlotController : Controller
    {
        private readonly Rppp15Context _context;

        public PlotController(Rppp15Context context)
        {
            _context = context;
        }

        // GET: Plot
        public async Task<IActionResult> Index()
        {
            var rppp15Context = _context.Plots.Include(p => p.InfrastructureNavigation).Include(p => p.Lease).Include(p => p.SoilQualityNavigation).Include(p => p.SoilTypeNavigation).Include(p => p.Subsidy).Include(p => p.SunlightNavigation);
            return View(await rppp15Context.ToListAsync());
        }
            

        // GET: Plot/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            var plot = await _context.Plots
                .Include(p => p.InfrastructureNavigation)
                .Include(p => p.Lease)
                .Include(p => p.SoilQualityNavigation)
                .Include(p => p.SoilTypeNavigation)
                .Include(p => p.Subsidy)
                .Include(p => p.SunlightNavigation)
                .FirstOrDefaultAsync(m => m.IdPlot == id);
            if (plot == null)
            {
                return NotFound();
            }

            return View(plot);
        }
        public async Task<JsonResult> GetPlotTags(string term)
        {
            var plots = await _context.Plots
                .Where(p => p.Tag.Contains(term))
                .Select(p => new { id = p.IdPlot, name = p.Tag })
                .ToListAsync();

            return Json(plots);
        }

        // GET: Plot/Create
        public IActionResult Create()
        {
            ViewData["Infrastructure"] = new SelectList(_context.Infrastructures, "Caption", "Caption");
            ViewData["LeaseId"] = new SelectList(_context.Leases, "IdLease", "IdLease");
            ViewData["SoilQuality"] = new SelectList(_context.SoilQualities, "Caption", "Caption");
            ViewData["SoilType"] = new SelectList(_context.SoilTypes, "Caption", "Caption");
            ViewData["SubsidyId"] = new SelectList(_context.Subsidies, "IdSubsidy", "IdSubsidy");
            ViewData["Sunlight"] = new SelectList(_context.Sunlights, "Caption", "Caption");
            return View();
        }

        // POST: Plot/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Latitude,Longitude,Tag,Area,IdPlot,SoilQuality,SoilType,Sunlight,Infrastructure,SubsidyId,LeaseId,InsertIntoRppp15DboPlot")] Plot plotController)
        {
            if (ModelState.IsValid)
            {
                _context.Add(plotController);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Infrastructure"] = new SelectList(_context.Infrastructures, "Caption", "Caption", plotController.Infrastructure);
            ViewData["LeaseId"] = new SelectList(_context.Leases, "IdLease", "IdLease", plotController.LeaseId);
            ViewData["SoilQuality"] = new SelectList(_context.SoilQualities, "Caption", "Caption", plotController.SoilQuality);
            ViewData["SoilType"] = new SelectList(_context.SoilTypes, "Caption", "Caption", plotController.SoilType);
            ViewData["SubsidyId"] = new SelectList(_context.Subsidies, "IdSubsidy", "IdSubsidy", plotController.SubsidyId);
            ViewData["Sunlight"] = new SelectList(_context.Sunlights, "Caption", "Caption", plotController.Sunlight);
            return View(plotController);
        }

        // GET: Plot/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plot = await _context.Plots.FindAsync(id);
            if (plot == null)
            {
                return NotFound();
            }
            ViewData["Infrastructure"] = new SelectList(_context.Infrastructures, "Caption", "Caption", plot.Infrastructure);
            ViewData["LeaseId"] = new SelectList(_context.Leases, "IdLease", "IdLease", plot.LeaseId);
            ViewData["SoilQuality"] = new SelectList(_context.SoilQualities, "Caption", "Caption", plot.SoilQuality);
            ViewData["SoilType"] = new SelectList(_context.SoilTypes, "Caption", "Caption", plot.SoilType);
            ViewData["SubsidyId"] = new SelectList(_context.Subsidies, "IdSubsidy", "IdSubsidy", plot.SubsidyId);
            ViewData["Sunlight"] = new SelectList(_context.Sunlights, "Caption", "Caption", plot.Sunlight);
            return View(plot);
        }

        // POST: Plot/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Latitude,Longitude,Tag,Area,IdPlot,SoilQuality,SoilType,Sunlight,Infrastructure,SubsidyId,LeaseId,InsertIntoRppp15DboPlot")] Plot plotController)
        {
            if (id != plotController.IdPlot)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(plotController);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlotExists(plotController.IdPlot))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Infrastructure"] = new SelectList(_context.Infrastructures, "Caption", "Caption", plotController.Infrastructure);
            ViewData["LeaseId"] = new SelectList(_context.Leases, "IdLease", "IdLease", plotController.LeaseId);
            ViewData["SoilQuality"] = new SelectList(_context.SoilQualities, "Caption", "Caption", plotController.SoilQuality);
            ViewData["SoilType"] = new SelectList(_context.SoilTypes, "Caption", "Caption", plotController.SoilType);
            ViewData["SubsidyId"] = new SelectList(_context.Subsidies, "IdSubsidy", "IdSubsidy", plotController.SubsidyId);
            ViewData["Sunlight"] = new SelectList(_context.Sunlights, "Caption", "Caption", plotController.Sunlight);
            return View(plotController);
        }

        // GET: Plot/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plot = await _context.Plots
                .Include(p => p.InfrastructureNavigation)
                .Include(p => p.Lease)
                .Include(p => p.SoilQualityNavigation)
                .Include(p => p.SoilTypeNavigation)
                .Include(p => p.Subsidy)
                .Include(p => p.SunlightNavigation)
                .FirstOrDefaultAsync(m => m.IdPlot == id);
            if (plot == null)
            {
                return NotFound();
            }

            return View(plot);
        }
        //Don't yet know where to put this, but I will need it for the popup confirmation of deletion
        //function fnConfirmDelete() {
        //   return confirm("Are you sure you want to delete this?");
        // }

        // POST: Plot/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
        
            var plot = await _context.Plots.FindAsync(id);
            if (plot != null)
            {
                _context.Plots.Remove(plot);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlotExists(int id)
        {
            return _context.Plots.Any(e => e.IdPlot == id);
        }
    }
}
