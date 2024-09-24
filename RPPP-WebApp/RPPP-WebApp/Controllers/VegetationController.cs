using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class VegetationController : Controller
    {
        private readonly Rppp15Context _context;

        public VegetationController(Rppp15Context context)
        {
            _context = context;
        }

        // GET: Vegetation
        public async Task<IActionResult> Index(int? plotId, int? plantClassId)
        {
            IQueryable<Vegetation> vegetationQuery = _context.Vegetations
                .Include(v => v.PlantClass)
                .Include(v => v.Plot);

            if (plotId.HasValue)
            {
                vegetationQuery = vegetationQuery.Where(v => v.PlotId == plotId);
            }
            if (plantClassId.HasValue)
            {
                vegetationQuery = vegetationQuery.Where(v => v.PlantClassId == plantClassId);
            }
            var viewModel = new VegetationViewModel()
            {
                Vegetations = await vegetationQuery.ToListAsync(),
                AllPlots = await _context.Plots.ToListAsync() // Fetch the list of plots for the dropdown filter
            };

            return View(viewModel);
        }


        // GET: Vegetation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Vegetations == null)
            {
                return NotFound();
            }

            var vegetation = await _context.Vegetations
                .Include(v => v.PlantClass)
                .Include(v => v.Plot)
                .FirstOrDefaultAsync(m => m.IdVegetation == id);
            if (vegetation == null)
            {
                return NotFound();
            }

            return View(vegetation);
        }

        // GET: Vegetation/Create
        public IActionResult Create()
        {
            ViewData["PlantClassId"] = new SelectList(_context.PlantClasses, "IdPlantClass", "Name");
            ViewData["PlotId"] = new SelectList(_context.Plots, "IdPlot", "IdPlot");
            return View();
        }

        // POST: Vegetation/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdVegetation,Units,PlantedOn,RemovedOn,YieldAnticipatedOn,ExpiryAnticipatedOn,PlotId,PlantClassId")] Vegetation vegetation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vegetation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PlantClassId"] = new SelectList(_context.PlantClasses, "IdPlantClass", "Name", vegetation.PlantClassId);
            ViewData["PlotId"] = new SelectList(_context.Plots, "IdPlot", "IdPlot", vegetation.PlotId);
            return View(vegetation);
        }

        // GET: Vegetation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Vegetations == null)
            {
                return NotFound();
            }

            var vegetation = await _context.Vegetations.FindAsync(id);
            if (vegetation == null)
            {
                return NotFound();
            }
            ViewData["PlantClassId"] = new SelectList(_context.PlantClasses, "IdPlantClass", "Name", vegetation.PlantClassId);
            ViewData["PlotId"] = new SelectList(_context.Plots, "IdPlot", "IdPlot", vegetation.PlotId);
            return View(vegetation);
        }

        // POST: Vegetation/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdVegetation,Units,PlantedOn,RemovedOn,YieldAnticipatedOn,ExpiryAnticipatedOn,PlotId,PlantClassId")] Vegetation vegetation)
        {
            if (id != vegetation.IdVegetation)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vegetation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VegetationExists(vegetation.IdVegetation))
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
            ViewData["PlantClassId"] = new SelectList(_context.PlantClasses, "IdPlantClass", "Name", vegetation.PlantClassId);
            ViewData["PlotId"] = new SelectList(_context.Plots, "IdPlot", "IdPlot", vegetation.PlotId);
            return View(vegetation);
        }

        // GET: Vegetation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Vegetations == null)
            {
                return NotFound();
            }

            var vegetation = await _context.Vegetations
                .Include(v => v.PlantClass)
                .Include(v => v.Plot)
                .FirstOrDefaultAsync(m => m.IdVegetation == id);
            if (vegetation == null)
            {
                return NotFound();
            }

            return View(vegetation);
        }

        // POST: Vegetation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Vegetations == null)
            {
                return Problem("Entity set 'Rppp15Context.Vegetations'  is null.");
            }
            var vegetation = await _context.Vegetations.FindAsync(id);
            if (vegetation != null)
            {
                _context.Vegetations.Remove(vegetation);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VegetationExists(int id)
        {
          return _context.Vegetations.Any(e => e.IdVegetation == id);
        }
    }
}
