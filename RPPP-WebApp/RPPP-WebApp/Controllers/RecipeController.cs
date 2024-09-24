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
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RecipeController : Controller
    {
        private readonly Rppp15Context _context;

        public RecipeController(Rppp15Context context)
        {
            _context = context;
        }

        // GET: Recipe
        public async Task<IActionResult> Index(int page = 1)
        {
            int itemsPerPage = 3;

            var totalItems = _context.Recipes.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);

            page = Math.Max(1, Math.Min(page, totalPages));

            var recipes = await _context.Recipes
                .Include(r => r.Cuisine)
                .OrderBy(r => r.IdRecipe)
                .Skip((page - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            return View(recipes);
        }


        public async Task<IActionResult> Details(int? id, string entityType)
        {
            if (id == null || entityType == null)
            {
                return NotFound();
            }

            var totalRecipes = _context.Recipes.Count();

            if (entityType.Equals("PlantClass", StringComparison.OrdinalIgnoreCase))
            {
                var plantClass = await _context.PlantClasses
                    .Include(p => p.Ingredients)
                    .ThenInclude(i => i.Recipe)
                    .FirstOrDefaultAsync(m => m.IdPlantClass == id);

                if (plantClass == null)
                {
                    return NotFound();
                }

                // Assuming there is only one ingredient associated with the plantClass
                var ingredient = plantClass.Ingredients.FirstOrDefault();

                if (ingredient == null)
                {
                    // Handle the case where the plantClass is not associated with any ingredient
                    return NotFound();
                }

                // Redirect to the associated recipe details page
                return RedirectToAction("Details", new { id = ingredient.Recipe.IdRecipe, entityType="Recipe" });
            }

            // If navigating directly from Recipe, retrieve the recipe with ingredients
            var recipe = await _context.Recipes
                .Include(r => r.Cuisine)
                .Include(r => r.Ingredients)
                .ThenInclude(i => i.PlantClass)
                .FirstOrDefaultAsync(m => m.IdRecipe == id);

            if (recipe == null)
            {
                return NotFound();
            }

            ViewData["CuisineId"] = new SelectList(_context.Cuisines, "IdCuisine", "Caption", recipe.CuisineId);
            ViewData["TotalRecipes"] = totalRecipes;

            return View(recipe);
        }







        // GET: Recipe/Create
        public IActionResult Create()
        {
            ViewData["CuisineId"] = new SelectList(_context.Cuisines, "IdCuisine", "Caption");
            return View();
        }

        // POST: Recipe/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdRecipe,Description,CaloriesPerServing,ApproximateDuration,CuisineId,Caption")] Recipe recipe)
        {
            if (ModelState.IsValid)
            {
                _context.Add(recipe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CuisineId"] = new SelectList(_context.Cuisines, "IdCuisine", "Caption", recipe.CuisineId);
            return View(recipe);
        }

        // GET: Recipe/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            int totalRecipes = _context.Recipes.Count();
            if (id == null || _context.Recipes == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }
            ViewData["CuisineId"] = new SelectList(_context.Cuisines, "IdCuisine", "Caption", recipe.CuisineId);
            ViewData["TotalRecipes"] = totalRecipes;
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // If AJAX, return the partial view for the recipe details
                return PartialView("Edit", recipe);
            }

            // Otherwise, return the regular view
            return View(recipe);
        }

        // POST: Recipe/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRecipe,Description,CaloriesPerServing,ApproximateDuration,CuisineId,Caption")] Recipe recipe)
        {
            if (id != recipe.IdRecipe)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recipe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecipeExists(recipe.IdRecipe))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // If AJAX, return the partial view for the recipe details
                    return PartialView("Edit", recipe);
                }

                // Otherwise, redirect to the recipe details using the associated ingredient's recipeId
                return RedirectToAction("Details", "Recipe", new { id = recipe.IdRecipe, entityType = "Recipe" });
            }
            ViewData["CuisineId"] = new SelectList(_context.Cuisines, "IdCuisine", "Caption", recipe.CuisineId);
            return View(recipe);
        }

        // GET: Recipe/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Recipes == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes
                .Include(r => r.Cuisine)
                .FirstOrDefaultAsync(m => m.IdRecipe == id);
            if (recipe == null)
            {
                return NotFound();
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // If AJAX, return the partial view for the delete confirmation
                return PartialView("Delete", recipe);
            }

            return View(recipe);
        }

        // POST: Recipe/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Recipes == null)
            {
                return Problem("Entity set 'Rppp15Context.Recipes'  is null.");
            }
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe != null)
            {
                _context.Recipes.Remove(recipe);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Recipe", new { id = recipe.IdRecipe, entityType = "Recipe" });
        }

        private bool RecipeExists(int id)
        {
          return _context.Recipes.Any(e => e.IdRecipe == id);
        }
    }
}
