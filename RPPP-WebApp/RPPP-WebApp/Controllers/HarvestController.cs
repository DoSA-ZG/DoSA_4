using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Controller for managing Harvest entities.
    /// </summary>
    public class HarvestController : Controller
    {
        private readonly Rppp15Context _context;
        /// <summary>
        /// Initializes a new instance of the <see cref="HarvestController"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>

        public HarvestController(Rppp15Context context)
        {
            _context = context;
        }

        // GET: Harvest
        /// <summary>
        /// Displays a paginated list of Harvest entities.
        /// </summary>
        /// <param name="page">The page number.</param>
        /// <returns>The view with the paginated list.</returns>
        public async Task<IActionResult> Index(int? page)
        {

            var rppp15Context = _context.Harvests.Include(h => h.Vegetation);

            int itemsPerPage = 3;
            int pageNumber = page ?? 1;

            int totalItems = rppp15Context.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);

            var paginatedData = rppp15Context.OrderBy(h => h.IdHarvest)
                                       .Skip((pageNumber - 1) * itemsPerPage)
                                       .Take(itemsPerPage)
                                       .ToList();
 
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;

            return View(paginatedData);
        }

        // GET: Harvest/Details/5
        /// <summary>
        /// Displays details of a specific Harvest entity.
        /// </summary>
        /// <param name="id">The ID of the Harvest entity.</param>
        /// <returns>The view with details.</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Harvests == null)
            {
                return NotFound();
            }

            var harvest = await _context.Harvests
                .Include(h => h.Vegetation)
                .FirstOrDefaultAsync(m => m.IdHarvest == id);
            if (harvest == null)
            {
                return NotFound();
            }

            var allIds = _context.Purchases.OrderBy(p => p.IdPurchase).Select(p => p.IdPurchase).ToList();
            int currentIndex = allIds.IndexOf(id.Value);

            // Calculate the previous and next IDs
            int previousId = currentIndex > 0 ? allIds[currentIndex - 1] : -1;
            int nextId = currentIndex < allIds.Count - 1 ? allIds[currentIndex + 1] : -1;
            int totalItems = _context.Harvests.Count();

            // Retrieving purchases related to this harvest
            List<Purchase> relatedPurchases = _context.Purchases.Where(p => p.HarvestId == id).ToList();

            var tupleModel = new ValueTuple<Harvest, List<Purchase>>(harvest, relatedPurchases);
            ViewBag.VegetationId = new SelectList(_context.Vegetations, "IdVegetation", "IdVegetation", harvest.VegetationId);
            ViewData["TotalItems"] = totalItems;
            ViewData["PreviousId"] = previousId;
            ViewData["NextId"] = nextId;
            return View(tupleModel);
        }

        // GET: Harvest/Create
        /// <summary>
        /// Displays the form to create a new Harvest entity.
        /// </summary>
        /// <returns>The view with the create form.</returns>
        public IActionResult Create()
        {
            ViewData["VegetationId"] = new SelectList(_context.Vegetations, "IdVegetation", "IdVegetation");
            return View();
        }

        // POST: Harvest/Create
        /// <summary>
        /// Handles the creation of a new Harvest entity.
        /// </summary>
        /// <param name="harvest">The Harvest entity to create.</param>
        /// <returns>Redirects to the index action if successful; otherwise, returns the create form.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdHarvest,CollectedOn,Weight,Tag,SelectedPlantClassName")] Harvest harvest)
        {
            if (ModelState.IsValid)
            {
                var vegetation = _context.Vegetations
                    .FirstOrDefault(v => v.PlantClass.Name == harvest.SelectedPlantClassName);

                if (vegetation == null)
                {
                    ModelState.AddModelError("SelectedPlantClassName", "Invalid Plant Class Name");
                    return View(harvest);
                }

                harvest.VegetationId = vegetation.IdVegetation;

                _context.Add(harvest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["VegetationId"] = new SelectList(_context.Vegetations, "IdVegetation", "IdVegetation", harvest.VegetationId);
            return View(harvest);
        }



        // GET: Harvest/Edit/5
        /// <summary>
        /// Displays the form to edit a specific Harvest entity.
        /// </summary>
        /// <param name="id">The ID of the Harvest entity to edit.</param>
        /// <returns>The partial view with the edit form.</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Harvests == null)
            {
                return NotFound();
            }

            var harvest = await _context.Harvests.FindAsync(id);
            if (harvest == null)
            {
                return NotFound();
            }
            ViewData["VegetationId"] = new SelectList(_context.Vegetations, "IdVegetation", "IdVegetation", harvest.VegetationId);
            return View("Edit", harvest);
        }

        // POST: Harvest/Edit/5
        /// <summary>
        /// Handles the editing of a specific Harvest entity.
        /// </summary>
        /// <param name="id">The ID of the Harvest entity to edit.</param>
        /// <param name="harvest">The edited Harvest entity.</param>
        /// <returns>Redirects to the details action if successful; otherwise, returns the edit form.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdHarvest,CollectedOn,Weight,Tag,VegetationId")] Harvest harvest)
        {
            var allIds = _context.Purchases.OrderBy(p => p.IdPurchase).Select(p => p.IdPurchase).ToList();
            int currentIndex = allIds.IndexOf(id);

            int previousId = currentIndex > 0 ? allIds[currentIndex - 1] : -1;
            int nextId = currentIndex < allIds.Count - 1 ? allIds[currentIndex + 1] : -1;
            int totalItems = _context.Harvests.Count();
            if (id != harvest.IdHarvest)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(harvest);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", "Harvest", new { id = harvest.IdHarvest, vegetationId = harvest.VegetationId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HarvestExists(harvest.IdHarvest))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewData["VegetationId"] = new SelectList(_context.Vegetations, "IdVegetation", "IdVegetation", harvest.VegetationId);
            ViewData["TotalItems"] = totalItems;
            ViewData["PreviousId"] = previousId;
            ViewData["NextId"] = nextId;
            return View(harvest);
        }

        // GET: Harvest/Delete/5
        /// <summary>
        /// Displays a confirmation form to delete a specific Harvest entity.
        /// </summary>
        /// <param name="id">The ID of the Harvest entity to delete.</param>
        /// <returns>The partial view with the delete confirmation form.</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Harvests == null)
            {
                return NotFound();
            }

            var harvest = await _context.Harvests
                .Include(h => h.Vegetation)
                .FirstOrDefaultAsync(m => m.IdHarvest == id);
            if (harvest == null)
            {
                return NotFound();
            }

            return PartialView("Delete", harvest);
        }
        /// <summary>
        /// Handles the deletion of a specific Harvest entity.
        /// </summary>
        /// <param name="id">The ID of the Harvest entity to delete.</param>
        /// <returns>Redirects to the index action after successful deletion.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Harvests == null)
            {
                return Problem("Entity set 'Rppp15Context.Harvests' is null.");
            }

            var harvest = await _context.Harvests.FindAsync(id);
            if (harvest != null)
            {
                _context.Harvests.Remove(harvest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Checks if a Harvest entity with the specified ID exists.
        /// </summary>
        /// <param name="id">The ID to check.</param>
        /// <returns>True if the Harvest entity exists; otherwise, false.</returns>
        private bool HarvestExists(int id)
        {
          return _context.Harvests.Any(e => e.IdHarvest == id);
        }
    }
}
