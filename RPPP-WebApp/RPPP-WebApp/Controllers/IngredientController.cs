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
    public class IngredientController : Controller
    {
        private readonly Rppp15Context _context;

        public IngredientController(Rppp15Context context)
        {
            _context = context;
        }

        // GET: Ingredient
        public async Task<IActionResult> Index(int page=1)
        {

            int itemsPerPage = 3;

            var totalItems = _context.Ingredients.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
            page = Math.Max(1, Math.Min(page, totalPages));
            var skipAmount = (page - 1) * itemsPerPage;
            var rppp15Context = _context.Ingredients
                 .OrderBy(i => i.IdIngredient)
                 .Include(i => i.PlantClass)
                 .Include(i => i.Recipe)
                 .Skip(skipAmount)
                 .Take(itemsPerPage)
                 .ToList();
            
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            return View(rppp15Context);
        }

        // GET: Ingredient/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Ingredients == null)
            {
                return NotFound();
            }

            var ingredient = await _context.Ingredients
                .Include(i => i.PlantClass)
                .Include(i => i.Recipe)
                .FirstOrDefaultAsync(m => m.IdIngredient == id);
            if (ingredient == null)
            {
                return NotFound();
            }

            return View(ingredient);
        }

        // GET: Ingredient/Create
        public IActionResult Create()
        {
            ViewData["PlantClassId"] = new SelectList(_context.PlantClasses, "IdPlantClass", "Name");
            ViewData["RecipeId"] = new SelectList(_context.Recipes, "IdRecipe", "Caption");
            return PartialView("Create");
        }

        // POST: Ingredient/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlantClassId,RecipeId")] Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ingredient);
                await _context.SaveChangesAsync();

                // Redirect to Recipe/Details/{recipeId} after successfully creating the ingredient
                return RedirectToAction("Details", "Recipe", new { id = ingredient.RecipeId, entityType = "Recipe" });
            }
            ViewData["PlantClassId"] = new SelectList(_context.PlantClasses, "IdPlantClass", "Name", ingredient.PlantClassId);
            ViewData["RecipeId"] = new SelectList(_context.Recipes, "IdRecipe", "Caption", ingredient.RecipeId);
            return View(ingredient);
        }
        


        // GET: Ingredient/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Ingredients == null)
            {
                return NotFound();
            }

            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }
            ViewData["PlantClassId"] = new SelectList(_context.PlantClasses, "IdPlantClass", "Name", ingredient.PlantClassId);
            ViewData["RecipeId"] = new SelectList(_context.Recipes, "IdRecipe", "Caption", ingredient.RecipeId);
            return View(ingredient);
        }

        // POST: Ingredient/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdIngredient,PlantClassId,RecipeId")] Ingredient ingredient)
        {
            if (id != ingredient.IdIngredient)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ingredient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IngredientExists(ingredient.IdIngredient))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Redirect to Recipe/Details/{recipeId} after successfully updating the ingredient
                return RedirectToAction("Details", "Recipe", new { id = ingredient.RecipeId });
            }
            ViewData["PlantClassId"] = new SelectList(_context.PlantClasses, "IdPlantClass", "Name", ingredient.PlantClassId);
            ViewData["RecipeId"] = new SelectList(_context.Recipes, "IdRecipe", "Caption", ingredient.RecipeId);
            return View(ingredient);
        }

        // GET: Ingredient/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Ingredients == null)
            {
                return NotFound();
            }

            var ingredient = await _context.Ingredients
                .Include(i => i.PlantClass)
                .Include(i => i.Recipe)
                .FirstOrDefaultAsync(m => m.IdIngredient == id);
            if (ingredient == null)
            {
                return NotFound();
            }

            return View(ingredient);
        }

        // POST: Ingredient/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Ingredients == null)
            {
                return Problem("Entity set 'Rppp15Context.Ingredients'  is null.");
            }
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient != null)
            {
                _context.Ingredients.Remove(ingredient);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IngredientExists(int id)
        {
          return _context.Ingredients.Any(e => e.IdIngredient == id);
        }

        public IActionResult GetRecipes(string term)
        {
            var recipes = _context.Recipes
                .Where(v => v.Caption.Contains(term))
                .Select(v => new { id = v.IdRecipe, label = v.Caption })
                .Distinct()
                .ToList();

            return Json(recipes);
        }

    }
}
