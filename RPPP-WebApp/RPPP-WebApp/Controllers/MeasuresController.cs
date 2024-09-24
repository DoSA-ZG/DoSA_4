using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System.Text.Json;

namespace RPPP_WebApp.Controllers
{
  [ApiExplorerSettings(IgnoreApi = true)]
  public class MeasuresController : Controller
  {
    private readonly Rppp15Context ctx;
    private readonly AppSettings appData;

    public MeasuresController(Rppp15Context ctx, IOptionsSnapshot<AppSettings> options)
    {
      this.ctx = ctx;
      appData = options.Value;
    }

    public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
    {
      var query = ctx.Measures.AsQueryable();

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
        return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
      }

      query = query.ApplySort(sort, ascending);

      var measures = await query
        .Skip((page - 1) * pagesize)
        .Take(pagesize)
        .Include(m => m.MeasureType)
        .Include(m => m.Vegetation)
        .Include(m => m.Worker)
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
          Worker = m.Worker == null ? null : new WorkerViewModel
          {
            IdWorker = m.Worker.IdWorker,
            DailyWage = m.Worker.DailyWage,
            Tag = m.Worker.Tag,
            Notes = m.Worker.Notes,
            Email = m.Worker.Email,
            Phone = m.Worker.Phone,
            WorkerTypeId = m.Worker.WorkerTypeId
          }
        })
        .ToListAsync();

      var model = new MeasuresViewModel
      {
        Measures = measures,
        PagingInfo = pagingInfo
      };

      TempData["sort"] = sort;
      TempData["ascending"] = ascending;
      TempData["page"] = page;

      ViewBag.Message = TempData[Constants.Message];
      ViewBag.ErrorOccurred = TempData[Constants.ErrorOccurred];
      return View(model);
    }

    private void PrepareDropDownLists()
    {
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
      var workers = ctx.Workers
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
      .ToList();
      ViewBag.Types = new SelectList(types, nameof(MeasureType.IdMeasureType), nameof(MeasureType.Caption));
      ViewBag.Vegetations = new SelectList(vegetations, nameof(VegetationViewModel.IdVegetation), nameof(VegetationViewModel.Summary));
      ViewBag.Workers = new SelectList(workers, nameof(WorkerViewModel.IdWorker), nameof(WorkerViewModel.Summary));
    }

    [HttpGet]
    public IActionResult Create()
    {
      PrepareDropDownLists();
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MeasureViewModel model)
    {
      if (ModelState.IsValid)
      {
        var measure = new Measure 
        {
          PerformedOn = model.PerformedOn,
          Description = model.Description,
          MeasureTypeId = model.MeasureTypeId,
          VegetationId = model.VegetationId,
          WorkerId = model.WorkerId,
          DurationMinutes = model.DurationMinutes
        };

        try
        {
          ctx.Add(measure);
          await ctx.SaveChangesAsync();

          TempData[Constants.Message] = $"Measure has been added with id = {measure.IdMeasure}";
          TempData[Constants.ErrorOccurred] = false;
          return RedirectToAction(nameof(Index), new { page = 1, sort = 1, ascending = false });
        }
        catch (Exception exc)
        {
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
          PrepareDropDownLists();
          return View(model);
        }
      }
      else
      {
        PrepareDropDownLists();
        return View(model);
      }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
      var measure = await ctx.Measures
        .AsNoTracking()
        .Where(m => m.IdMeasure == id)
        .Include(m => m.MeasureType)
        .SingleOrDefaultAsync();

      if (measure != null)
      {
        var model = new MeasureViewModel
        {
          IdMeasure = measure.IdMeasure,
          PerformedOn = measure.PerformedOn,
          Description = measure.Description,
          MeasureTypeId = measure.MeasureTypeId,
          VegetationId = measure.VegetationId,
          WorkerId = measure.WorkerId,
          DurationMinutes = measure.DurationMinutes,
          MeasureTypeCaption = measure.MeasureType.Caption,
        };
        PrepareDropDownLists();
        return PartialView(model);
      }
      else
      {
        return NotFound($"Invalid measure id: {id}");
      }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(MeasureViewModel model)
    {
      if (model == null)
      {
        return NotFound("No data submitted!?");
      }
      var measure = await ctx.Measures.FindAsync(model.IdMeasure);
      if (measure == null)
      {
        return NotFound($"Invalid measure worker id: {model?.IdMeasure}");
      }

      ActionResponseMessage responseMessage;
      if (ModelState.IsValid)
      {
        measure.PerformedOn = model.PerformedOn;
        measure.Description = model.Description;
        measure.MeasureTypeId = model.MeasureTypeId;
        measure.VegetationId = model.VegetationId;
        measure.WorkerId = model.WorkerId;
        measure.DurationMinutes = model.DurationMinutes;

        try
        {
          await ctx.SaveChangesAsync();
          responseMessage = new ActionResponseMessage(MessageType.Success, $"Measure {measure.IdMeasure} has been successfully edited.");
        }
        catch (Exception exc)
        {
          responseMessage = new ActionResponseMessage(MessageType.Error, $"Error editing measure {measure.IdMeasure} {exc.CompleteExceptionMessage()}");
          PrepareDropDownLists();
        }
      }
      else
      {
        responseMessage = new ActionResponseMessage(MessageType.Error, $"Invalid data.");
        PrepareDropDownLists();
      }

      Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
      if (responseMessage.MessageType == MessageType.Success) {
        return RedirectToAction(nameof(Get), new { id = measure.IdMeasure });
      } else {
        return PartialView(model);
      }
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
      var measure = await ctx.Measures
        .Where(d => d.IdMeasure == id)
        .Include(m => m.MeasureType)
        .Include(m => m.Vegetation)
        .Include(m => m.Worker)
        .Include(m => m.Vegetation.PlantClass)
        .SingleOrDefaultAsync();
      if (measure == null) {
        return NotFound($"Invalid measure id: {id}");
      } else {
        var model = new MeasureViewModel
        {
          IdMeasure = measure.IdMeasure,
          PerformedOn = measure.PerformedOn,
          Description = measure.Description,
          MeasureTypeId = measure.MeasureTypeId,
          VegetationId = measure.VegetationId,
          WorkerId = measure.WorkerId,
          DurationMinutes = measure.DurationMinutes,
          MeasureTypeCaption = measure.MeasureType.Caption,
          Vegetation = new VegetationViewModel
          {
            IdVegetation = measure.Vegetation.IdVegetation,
            Units = measure.Vegetation.Units,
            PlantedOn = measure.Vegetation.PlantedOn,
            RemovedOn = measure.Vegetation.RemovedOn,
            YieldAnticipatedOn = measure.Vegetation.YieldAnticipatedOn,
            ExpiryAnticipatedOn = measure.Vegetation.ExpiryAnticipatedOn,
            PlotId = measure.Vegetation.PlotId,
            PlantClassId = measure.Vegetation.PlantClassId,
            PlantClassName = measure.Vegetation.PlantClass.Name
          },
          Worker = measure.Worker == null ? null : new WorkerViewModel
          {
            IdWorker = measure.Worker.IdWorker,
            DailyWage = measure.Worker.DailyWage,
            Tag = measure.Worker.Tag,
            Notes = measure.Worker.Notes,
            Email = measure.Worker.Email,
            Phone = measure.Worker.Phone,
            WorkerTypeId = measure.Worker.WorkerTypeId
          }
        };
        return PartialView(model);
      }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
      ActionResponseMessage responseMessage;
      var measure = await ctx.Measures
                              .Where(d => d.IdMeasure == id)
                              .SingleOrDefaultAsync();
      if (measure != null)
      {
        try
        {
          ctx.Remove(measure);
          await ctx.SaveChangesAsync();
          responseMessage = new ActionResponseMessage(MessageType.Success, $"Measure {measure.IdMeasure} deleted.");
        }
        catch (Exception exc)
        {
          responseMessage = new ActionResponseMessage(MessageType.Error, $"Error deleting measure {measure.IdMeasure} {exc.CompleteExceptionMessage()}");
        }
      }
      else
      {
        responseMessage = new ActionResponseMessage(MessageType.Error, $"Invalid measure id: {id}");
      }

      Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
      return responseMessage.MessageType == MessageType.Error ? RedirectToAction(nameof(Get), new { id = measure.IdMeasure }) : new EmptyResult();
    }
  }
}