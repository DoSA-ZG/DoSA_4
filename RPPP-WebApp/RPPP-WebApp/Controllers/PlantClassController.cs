using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Controller for managing plant classes.
    /// </summary>
    public class PlantClassController : Controller
    {
        private readonly Rppp15Context _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlantClassController"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>

        public PlantClassController(Rppp15Context context)
        {
            _context = context;
        }
        // GET: PlantClass

        /// <summary>
        /// Displays a paginated list of plant classes.
        /// </summary>
        /// <param name="page">The page number.</param>
        /// <returns>Returns a paginated view of plant classes.</returns>
        public async Task<IActionResult> Index(int page=1)
        {
            int itemsPerPage = 3;

            var totalItems = _context.PlantClasses.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
            page = Math.Max(1, Math.Min(page, totalPages));

            var skipAmount = (page - 1) * itemsPerPage;
            var rppp15Context = _context.PlantClasses.OrderBy(p => p.IdPlantClass) 
                .Include(p => p.Parent)
                .Skip(skipAmount)
                .Take(itemsPerPage)
                .ToList();
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            return View(rppp15Context);
        }
        public async Task<JsonResult> GetPlantClassNames(string term)
        {
            var plantClasses = await _context.PlantClasses
                .Where(pc => pc.Name.Contains(term))
                .Select(pc => new { id = pc.IdPlantClass, name = pc.Name })
                .ToListAsync();

            return Json(plantClasses);
        }


        // GET: PlantClass/Details/5
        /// <summary>
        /// Displays details of a specific plant class.
        /// </summary>
        /// <param name="id">The ID of the plant class.</param>
        /// <returns>Returns the details view for a specific plant class.</returns>
        public async Task<IActionResult> Details(int? id)
        {
            int totalItems = _context.PlantClasses.Count();

            if (id == null || _context.PlantClasses == null)
            {
                return NotFound();
            }

            var plantClass = await _context.PlantClasses
                .Include(p => p.Parent)
                .FirstOrDefaultAsync(m => m.IdPlantClass == id);

            if (plantClass == null)
            {
                return NotFound();
            }

            return PartialView("Details", plantClass);
        }


        // GET: PlantClass/Create

        /// <summary>
        /// Displays the form for creating a new plant class.
        /// </summary>
        /// <returns>Returns the create view for a new plant class.</returns>
        public IActionResult Create()
        {
            ViewData["ParentId"] = new SelectList(_context.PlantClasses, "IdPlantClass", "Name");
            return View();
        }

        // POST: PlantClass/Create
        /// <summary>
        /// Handles the HTTP POST request for creating a new plant class.
        /// </summary>
        /// <param name="plantClass">The plant class data from the form.</param>
        /// <returns>Returns the index view if successful, otherwise returns the create view with errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPlantClass,Name,Passport,ParentId,FiberPerServing,PotassiumPerServing")] PlantClass plantClass)
        {
            if (ModelState.IsValid)
            {
                _context.Add(plantClass);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentId"] = new SelectList(_context.PlantClasses, "IdPlantClass", "Name", plantClass.ParentId);
            return View(plantClass);
        }

        // GET: PlantClass/Edit/5
        /// <summary>
        /// Displays the form for editing an existing plant class.
        /// </summary>
        /// <param name="id">The ID of the plant class to edit.</param>
        /// <returns>Returns the edit view for the specified plant class.</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            int totalItems = _context.Recipes.Count();
            if (id == null || _context.PlantClasses == null)
            {
                return NotFound();
            }

            var plantClass = await _context.PlantClasses.FindAsync(id);
            if (plantClass == null)
            {
                return NotFound();
            }

            ViewData["ParentId"] = new SelectList(_context.PlantClasses, "IdPlantClass", "Name", plantClass.ParentId);
            ViewData["TotalItems"] = totalItems;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Edit", plantClass);
            }
            return View(plantClass);
        }

        // POST: PlantClass/Edit/5
        /// <summary>
        /// Handles the HTTP POST request for editing an existing plant class.
        /// </summary>
        /// <param name="id">The ID of the plant class to edit.</param>
        /// <param name="plantClass">The updated plant class data from the form.</param>
        /// <returns>Returns the index view if successful, otherwise returns the edit view with errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPlantClass,Name,Passport,ParentId,FiberPerServing,PotassiumPerServing")] PlantClass plantClass)
        {
            if (id != plantClass.IdPlantClass)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(plantClass);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlantClassExists(plantClass.IdPlantClass))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                var ingredient = _context.Ingredients.FirstOrDefault(i => i.PlantClassId == plantClass.IdPlantClass);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("Edit", plantClass);
                }

                return RedirectToAction("Details", "Recipe", new { id = ingredient?.RecipeId, entityType = "Recipe" });
            }

            ViewData["ParentId"] = new SelectList(_context.PlantClasses, "IdPlantClass", "Name", plantClass.ParentId);
            return View(plantClass);
        }




        // GET: PlantClass/Delete/5
        /// <summary>
        /// Displays the confirmation page for deleting a plant class.
        /// </summary>
        /// <param name="id">The ID of the plant class to delete.</param>
        /// <returns>Returns the delete view for the specified plant class.</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PlantClasses == null)
            {
                return NotFound();
            }

            var plantClass = await _context.PlantClasses
                .Include(p => p.Parent)
                .FirstOrDefaultAsync(m => m.IdPlantClass == id);
            if (plantClass == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Delete", plantClass);
            }
            return View(plantClass);
        }

        // POST: PlantClass/Delete/5
        /// <summary>
        /// Handles the HTTP POST request for deleting a plant class.
        /// </summary>
        /// <param name="id">The ID of the plant class to delete.</param>
        /// <returns>Returns the details view for the associated recipe after deletion.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PlantClasses == null)
            {
                return Problem("Entity set 'Rppp15Context.PlantClasses'  is null.");
            }
            var plantClass = await _context.PlantClasses.FindAsync(id);
            var ingredient = _context.Ingredients.FirstOrDefault(i => i.PlantClassId == plantClass.IdPlantClass);
            if (plantClass != null)
            {
                _context.PlantClasses.Remove(plantClass);
            }
            
            await _context.SaveChangesAsync();


            return RedirectToAction("Details", "Recipe", new { id = ingredient?.RecipeId, entityType = "Recipe" });
        }
        /// <summary>
        /// Checks if a plant class with the specified ID exists.
        /// </summary>
        /// <param name="id">The ID of the plant class to check.</param>
        /// <returns>Returns true if the plant class exists, false otherwise.</returns>
        private bool PlantClassExists(int id)
        {
          return _context.PlantClasses.Any(e => e.IdPlantClass == id);
        }
    }
}
