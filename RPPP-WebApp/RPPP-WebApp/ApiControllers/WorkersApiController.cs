using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;

namespace RPPP_WebApp.ApiControllers
{
  /// <summary>
  /// Web API service for workers CRUD operations
  /// </summary>
  [ApiController]
  [Route("/api/workers")]
  [Produces("application/json")]
  public class WorkersApiController : ControllerBase
  {
    private readonly Rppp15Context ctx;
    private readonly AppSettings appData;

    /// <summary>
    /// Create an instance of the controller, injecting dependencies 
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="options"></param>
    public WorkersApiController(Rppp15Context ctx, IOptionsSnapshot<AppSettings> options) 
    {
      this.ctx = ctx;
      appData = options.Value;
    }

    /// <summary>
    /// Get the overall number of workers
    /// </summary>
    /// <returns></returns>
    [HttpGet("count")]
    public async Task<int> Count() {
      var query = ctx.Workers.AsQueryable();
      return await query.CountAsync();
    }

    /// <summary>
    /// Get a list of workers with pagination and sorting applied
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="sort">Index of the field to sort by</param>
    /// <param name="ascending">Whether the sort should be ascending</param>
    /// <returns>A list of paginated and sorted measures</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<WorkerViewModel>>> GetAll(int page = 1, int sort = 1, bool ascending = true) {
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
        return BadRequest();
      }

      query = query.ApplySort(sort, ascending);

      var workers = await query
        .Skip((page - 1) * pagesize)
        .Take(pagesize)
        .Include(w => w.WorkerType)
        .Select(w => new WorkerViewModel
        {
          IdWorker = w.IdWorker,
          DailyWage = w.DailyWage,
          Tag = w.Tag,
          Notes = w.Notes,
          Email = w.Email,
          Phone = w.Phone,
          WorkerTypeId = w.WorkerTypeId,
          WorkerTypeCaption = w.WorkerType != null ? w.WorkerType.Caption : null,
        })
        .ToListAsync();

        return workers;
    }

    /// <summary>
    /// Get a single worker by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkerViewModel>> Get(int id) {
      var workerModel = await ctx.Workers
        .Where(d => d.IdWorker == id)
        .Include(w => w.WorkerType)
        .Select(w => new WorkerViewModel
        {
          IdWorker = w.IdWorker,
          DailyWage = w.DailyWage,
          Tag = w.Tag,
          Notes = w.Notes,
          Email = w.Email,
          Phone = w.Phone,
          WorkerTypeId = w.WorkerTypeId,
          WorkerTypeCaption = w.WorkerType != null ? w.WorkerType.Caption : null,
        })
        .SingleOrDefaultAsync();
      if (workerModel == null) {
        return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid measure id: {id}");
      } else {
        return workerModel;
      }
    }

    /// <summary>
    /// Get measures performed by a worker with a certain id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/measures")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<MeasureViewModel>> GetMeasures(int id) {
      var measures = await ctx.Measures
        .Where(m => m.WorkerId == id)
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

    /// <summary>
    /// Delete a worker by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id) {
      var worker = await ctx.Workers.FindAsync(id);

      if (worker == null) {
        return NotFound();
      } else {
        ctx.Remove(worker);
        await ctx.SaveChangesAsync();
        return NoContent();
      }
    }

    /// <summary>
    /// Edit an existing worker
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model">Worker data, IDs must match</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, WorkerViewModel model) {
        if (model.IdWorker != id)
        {
          return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Different ids {id} vs {model.IdWorker}");
        } else {
          var worker = await ctx.Workers.FindAsync(id);
          if (worker == null) {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid worker id: {id}");
          }

            worker.DailyWage = model.DailyWage;
            worker.Tag = model.Tag;
            worker.Notes = model.Notes;
            worker.Email = model.Email;
            worker.Phone = model.Phone;
            worker.WorkerTypeId = model.WorkerTypeId;

          await ctx.SaveChangesAsync();
          return NoContent();
        }
    }

    /// <summary>
    /// Creates a new worker
    /// </summary>
    /// <param name="model">Worker data; Only daily wage is required</param>
    /// <returns></returns>
    [HttpPost()]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(WorkerViewModel model) {
      var worker = new Worker
        {
          DailyWage = model.DailyWage,
          Tag = model.Tag,
          Notes = model.Notes,
          Email = model.Email,
          Phone = model.Phone,
          WorkerTypeId = model.WorkerTypeId
        };

      ctx.Add(worker);
      await ctx.SaveChangesAsync();

      var newWorker = await Get(worker.IdWorker);

      return CreatedAtAction(nameof(Get), new { id = worker.IdWorker }, newWorker.Value);
    }
  }
}