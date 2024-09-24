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
    public class PurchaseController : Controller
    {
        private readonly Rppp15Context _context;

        public PurchaseController(Rppp15Context context)
        {
            _context = context;
        }

        // GET: Purchase
        public async Task<IActionResult> Index(int? page)
        {
            var rppp15Context = _context.Purchases.Include(p => p.Harvest).Include(p => p.Order);
            int itemsPerPage = 3;
            int pageNumber = page ?? 1; // Default to the first page if page is null
         
            int totalItems = rppp15Context.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);

            var paginatedData = rppp15Context.OrderBy(h => h.IdPurchase)
                                       .Skip((pageNumber - 1) * itemsPerPage)
                                       .Take(itemsPerPage)
                                       .ToList();

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = pageNumber;
            return View(paginatedData);
        }

        // GET: Purchase/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Purchases == null)
            {
                return NotFound();
            }

            var purchase = await _context.Purchases
                .Include(p => p.Harvest)
                .Include(p => p.Order)
                .FirstOrDefaultAsync(m => m.IdPurchase == id);
            if (purchase == null)
            {
                return NotFound();
            }

            return PartialView("Details", purchase);
        }

        // GET: Purchase/CreateForm
        public IActionResult CreateForm(int? harvestId)
        {
            ViewData["HarvestId"] = new SelectList(_context.Harvests, "IdHarvest", "IdHarvest");
            ViewData["OrderId"] = new SelectList(_context.Orders, "IdOrder", "Description");
            ViewData["SelectedHarvestId"] = harvestId;
            return PartialView("_CreateForm");  // Return a partial view
        }



        // POST: Purchase/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // PurchaseController.cs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPurchase,CollectedOn,Weight,Gain,Tag,OrderId,HarvestId")] Purchase purchase)
        {
            if (ModelState.IsValid)
            {
                _context.Add(purchase);
                await _context.SaveChangesAsync();

                // Clear ModelState to avoid issues when rendering the view again
                ModelState.Clear();

                // If the model state is valid, redirect to Harvest details
                return RedirectToAction("Details", "Harvest", new { id = purchase.HarvestId, purchaseId = purchase.IdPurchase });
            }

            // If the model state is not valid, return the create form again
            ViewData["HarvestId"] = new SelectList(_context.Harvests, "IdHarvest", "IdHarvest");
            ViewData["OrderId"] = new SelectList(_context.Orders, "IdOrder", "Description");
            ViewData["SelectedHarvestId"] = purchase.HarvestId; // Set the selected harvestId
            return View(purchase);
        }






        // GET: Purchase/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Purchases == null)
            {
                return NotFound();
            }

            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
            {
                return NotFound();
            }
            ViewData["HarvestId"] = new SelectList(_context.Harvests, "IdHarvest", "IdHarvest", purchase.HarvestId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "IdOrder", "Description", purchase.OrderId);
            return PartialView("Edit", purchase);
        }

        // POST: Purchase/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPurchase,CollectedOn,Weight,Gain,Tag,OrderId,HarvestId")] Purchase purchase)
        {
            if (id != purchase.IdPurchase)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(purchase);
                    await _context.SaveChangesAsync();

                    // Redirect to the harvest details page with both HarvestId and IdPurchase
                    return RedirectToAction("Details", "Harvest", new { id = purchase.HarvestId, purchaseId = purchase.IdPurchase });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseExists(purchase.IdPurchase))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["HarvestId"] = new SelectList(_context.Harvests, "IdHarvest", "IdHarvest", purchase.HarvestId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "IdOrder", "Description", purchase.OrderId);

            // If there are validation errors, return to the same page
            return View(purchase);
        }



        // GET: Purchase/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Purchases == null)
            {
                return NotFound();
            }

            var purchase = await _context.Purchases
                .Include(p => p.Harvest)
                .Include(p => p.Order)
                .FirstOrDefaultAsync(m => m.IdPurchase == id);
            if (purchase == null)
            {
                return NotFound();
            }

            return PartialView("Delete", purchase);
        }

        // POST: Purchase/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var purchase = await _context.Purchases.FindAsync(id);

            if (purchase == null)
            {
                return NotFound();
            }

            // Store the HarvestId before removing the purchase
            int harvestId = purchase.HarvestId;

            _context.Purchases.Remove(purchase);

            await _context.SaveChangesAsync();
            // Redirect to the Harvest Details page after deletion
            return RedirectToAction("Details", "Harvest", new { id = harvestId });
        }



        private bool PurchaseExists(int id)
        {
          return _context.Purchases.Any(e => e.IdPurchase == id);
        }
    }
}
