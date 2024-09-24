using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{
  [ApiExplorerSettings(IgnoreApi = true)]
  public class WorkersController : Controller
  {
    private readonly Rppp15Context ctx;
    private readonly AppSettings appData;

    public WorkersController(Rppp15Context ctx, IOptionsSnapshot<AppSettings> options)
    {
      this.ctx = ctx;
      appData = options.Value;
    }

    private async Task<ICollection<MeasureViewModel>> getMeasures(WorkerViewModel worker)
    {
      var measures = await ctx.Measures
        .Where(m => m.WorkerId == worker.IdWorker)
        .Include(m => m.MeasureType)
        .Include(m => m.Vegetation)
        .Include(m => m.Vegetation.PlantClass)
        .Select(m => new MeasureViewModel
        {
          IdMeasure = m.IdMeasure,
          PerformedOn = m.PerformedOn,
          Description = m.Description,
          MeasureTypeId = m.MeasureTypeId,
          VegetationId = m.VegetationId,
          WorkerId = m.WorkerId,
          DurationMinutes = m.DurationMinutes,
          MeasureTypeCaption = m.MeasureType.Caption,
          Vegetation = new VegetationViewModel
          {
            IdVegetation = m.Vegetation.IdVegetation,
            Units = m.Vegetation.Units,
            PlantedOn = m.Vegetation.PlantedOn,
            RemovedOn = m.Vegetation.RemovedOn,
            YieldAnticipatedOn = m.Vegetation.YieldAnticipatedOn,
            ExpiryAnticipatedOn = m.Vegetation.ExpiryAnticipatedOn,
            PlotId = m.Vegetation.PlotId,
            PlantClassId = m.Vegetation.PlantClassId,
            PlantClassName = m.Vegetation.PlantClass.Name
          },
        })
        .ToListAsync();

      return measures;
    }

    public async Task<IActionResult> Index(bool complex = false, int page = 1, int sort = 1, bool ascending = true)
    {
      var query = ctx.Workers.AsQueryable();

      int pagesize = appData.PageSize;
      int count = await query.CountAsync();

      var pagingInfo = new PagingInfo
      {
        CurrentPage = page,
        Sort = sort,
        Ascending = ascending,
        ItemsPerPage = pagesize,
        TotalItems = count
      };

      if (count > 0 && (page < 1 || page > pagingInfo.TotalPages))
      {
        return RedirectToAction(nameof(Index), new { page = 1, sort, ascending, complex });
      }

      query = query.ApplySort(sort, ascending);

      var workers = await query
        .Skip((page - 1) * pagesize)
        .Take(pagesize)
        .Select(w => new WorkerViewModel
        {
          IdWorker = w.IdWorker,
          DailyWage = w.DailyWage,
          Tag = w.Tag,
          Notes = w.Notes,
          Email = w.Email,
          Phone = w.Phone,
          WorkerTypeId = w.WorkerTypeId
        })
        .ToListAsync();

      foreach (var worker in workers)
      {
        if (complex)
        {
          worker.Measures = await getMeasures(worker);
        }

        if (worker.WorkerTypeId == null) continue;
        worker.WorkerTypeCaption = await ctx.WorkerTypes
          .Where(t => t.IdWorkerType == worker.WorkerTypeId)
          .Select(t => t.Caption)
          .FirstOrDefaultAsync();
      }

      var model = new WorkersViewModel
      {
        Workers = workers,
        PagingInfo = pagingInfo
      };

      TempData["complex"] = complex;
      TempData["sort"] = sort;
      TempData["ascending"] = ascending;
      TempData["page"] = page;

      ViewBag.Message = TempData[Constants.Message];
      ViewBag.ErrorOccurred = TempData[Constants.ErrorOccurred];
      return View(model);
    }

    private void PrepareMeasureDropdowns() {
      var types = ctx.MeasureTypes.ToList();
      var vegetations = ctx.Vegetations
      .Include(v => v.PlantClass)
      .Select(v => new VegetationViewModel
      {
        IdVegetation = v.IdVegetation,
        Units = v.Units,
        PlantedOn = v.PlantedOn,
        RemovedOn = v.RemovedOn,
        YieldAnticipatedOn = v.YieldAnticipatedOn,
        ExpiryAnticipatedOn = v.ExpiryAnticipatedOn,
        PlotId = v.PlotId,
        PlantClassId = v.PlantClassId,
        PlantClassName = v.PlantClass.Name
      })
      .ToList();
      ViewBag.MeasureTypes = new SelectList(types, nameof(MeasureType.IdMeasureType), nameof(MeasureType.Caption));
      ViewBag.Vegetations = new SelectList(vegetations, nameof(VegetationViewModel.IdVegetation), nameof(VegetationViewModel.Summary));
    }

    private async Task<List<Measure>> GetNewMeasures(WorkerViewModel model) {
      List<Measure> newMeasures = new();

      foreach (var measure in model.Measures) {
        Measure editedMeasure;
        if (measure.IdMeasure > 0) {
          editedMeasure = await ctx.Measures.FindAsync(measure.IdMeasure);
        } else {
          editedMeasure = new Measure();
        }
        editedMeasure.PerformedOn = measure.PerformedOn;
        editedMeasure.Description = measure.Description;
        editedMeasure.MeasureTypeId = measure.MeasureTypeId;
        editedMeasure.VegetationId = measure.VegetationId;
        editedMeasure.WorkerId = measure.WorkerId;
        editedMeasure.DurationMinutes = measure.DurationMinutes;

        newMeasures.Add(editedMeasure);
      }

      return newMeasures;
    }

    [HttpGet]
    public IActionResult Create()
    {
      PrepareMeasureDropdowns();
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkerViewModel model)
    {
      if (ModelState.IsValid)
      {
        var worker = new Worker
        {
          DailyWage = model.DailyWage,
          Tag = model.Tag,
          Notes = model.Notes,
          Email = model.Email,
          Phone = model.Phone,
          WorkerTypeId = model.WorkerTypeId,
          Measures = await GetNewMeasures(model)
        };

        try
        {
          ctx.Add(worker);
          await ctx.SaveChangesAsync();

          TempData[Constants.Message] = $"Worker has been added with id = {worker.IdWorker}";
          TempData[Constants.ErrorOccurred] = false;
          return RedirectToAction(nameof(Index), new { complex = TempData["complex"], page = 1, sort = 1, ascending = false });
        }
        catch (Exception exc)
        {
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
          PrepareMeasureDropdowns();
          return View(model);
        }
      }
      else
      {
        PrepareMeasureDropdowns();
        return View(model);
      }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
      TempData.Keep();

      var worker = await ctx.Workers
        .AsNoTracking()
        .Include(w => w.WorkerType)
        .Where(w => w.IdWorker == id)
        .SingleOrDefaultAsync();

      if (worker != null)
      {
        var model = new WorkerViewModel
        {
          IdWorker = worker.IdWorker,
          DailyWage = worker.DailyWage,
          Tag = worker.Tag,
          Notes = worker.Notes,
          Email = worker.Email,
          Phone = worker.Phone,
          WorkerTypeId = worker.WorkerTypeId,
          WorkerTypeCaption = worker.WorkerType != null ? worker.WorkerType.Caption : null,
        };
        model.Measures = await getMeasures(model);
        PrepareMeasureDropdowns();

        ViewBag.Message = TempData[Constants.Message];
        ViewBag.ErrorOccurred = TempData[Constants.ErrorOccurred];

        return View(model);
      }
      else
      {
        return NotFound($"Invalid worker id: {id}");
      }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(WorkerViewModel model)
    {
      if (model == null)
      {
        return NotFound("No data submitted!?");
      }
      var worker = await ctx.Workers
      .Include(w => w.Measures)
      .Where(w => w.IdWorker == model.IdWorker)
      .FirstOrDefaultAsync();
      if (worker == null)
      {
        return NotFound($"Invalid worker id: {model?.IdWorker}");
      }

      if (ModelState.IsValid)
      {
        worker.DailyWage = model.DailyWage;
        worker.Tag = model.Tag;
        worker.Notes = model.Notes;
        worker.Email = model.Email;
        worker.Phone = model.Phone;
        worker.WorkerTypeId = model.WorkerTypeId;

        worker.Measures = await GetNewMeasures(model);

        try
        {
          await ctx.SaveChangesAsync();
          TempData[Constants.Message] = $"Worker {worker.IdWorker} has been successfully edited.";
          TempData[Constants.ErrorOccurred] = false;
          return RedirectToAction(nameof(Edit), new { id = worker.IdWorker });
        }
        catch (Exception exc)
        {
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
          PrepareMeasureDropdowns();
          return View(model);
        }
      }
      else
      {
        PrepareMeasureDropdowns();
        return View(model);
      }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
      var worker = await ctx.Workers
                              .Where(d => d.IdWorker == id)
                              .SingleOrDefaultAsync();
      if (worker != null)
      {
        try
        {
          ctx.Remove(worker);
          await ctx.SaveChangesAsync();
          TempData[Constants.Message] = $"Worker {worker.IdWorker} deleted.";
          TempData[Constants.ErrorOccurred] = false;
        }
        catch (Exception exc)
        {
          TempData[Constants.Message] = $"Error deleting worker {worker.IdWorker} {exc.CompleteExceptionMessage()}";
          TempData[Constants.ErrorOccurred] = true;
        }
      }
      else
      {
        TempData[Constants.Message] = "Invalid worker id: " + id;
        TempData[Constants.ErrorOccurred] = true;
      }
      return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
      TempData.Keep();

      var worker = await ctx.Workers
        .AsNoTracking()
        .Where(w => w.IdWorker == id)
        .Include(w => w.WorkerType)
        .SingleOrDefaultAsync();

      if (worker != null)
      {
        var model = new WorkerViewModel
        {
          IdWorker = worker.IdWorker,
          DailyWage = worker.DailyWage,
          Tag = worker.Tag,
          Notes = worker.Notes,
          Email = worker.Email,
          Phone = worker.Phone,
          WorkerTypeId = worker.WorkerTypeId,
          WorkerTypeCaption = worker.WorkerType != null ? worker.WorkerType.Caption : null,
        };
        model.Measures = await getMeasures(model);
        return View(model);
      }
      else
      {
        return NotFound($"Invalid worker id: {id}");
      }
    }
  }
}