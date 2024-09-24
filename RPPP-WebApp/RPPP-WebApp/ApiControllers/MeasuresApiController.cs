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
  /// Web API service for measures CRUD operations
  /// </summary>
  [ApiController]
  [Route("/api/measures")]
  [Produces("application/json")]
  public class MeasuresApiController : ControllerBase
  {
    private readonly Rppp15Context ctx;
    private readonly AppSettings appData;

    /// <summary>
    /// Create an instance of the controller, injecting dependencies 
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="options"></param>
    public MeasuresApiController(Rppp15Context ctx, IOptionsSnapshot<AppSettings> options) 
    {
      this.ctx = ctx;
      appData = options.Value;
    }

    /// <summary>
    /// Get the overall number of measures
    /// </summary>
    /// <returns></returns>
    [HttpGet("count")]
    public async Task<int> Count() {
      var query = ctx.Measures.AsQueryable();
      return await query.CountAsync();
    }

    /// <summary>
    /// Get a list of measures with pagination and sorting applied
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="sort">Index of the field to sort by</param>
    /// <param name="ascending">Whether the sort should be ascending</param>
    /// <returns>A list of paginated and sorted measures</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<MeasureViewModel>>> GetAll(int page = 1, int sort = 1, bool ascending = true) {
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
        return BadRequest();
      }

      query = query.ApplySort(sort, ascending);

      var measures = await query
        .Skip((page - 1) * pagesize)
        .Take(pagesize)
        .Include(m => m.MeasureType)
        .Include(m => m.Vegetation)
        .Include(m => m.Worker)
        .Include(m => m.Vegetation.PlantClass)
        .Include(m => m.Worker.WorkerType)
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
            WorkerTypeId = m.Worker.WorkerTypeId,
            WorkerTypeCaption = m.Worker.WorkerType != null ? m.Worker.WorkerType.Caption : null
          }
        })
        .ToListAsync();

        return measures;
    }

    /// <summary>
    /// Get a single measure by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MeasureViewModel>> Get(int id) {
      var measure = await ctx.Measures
        .Where(d => d.IdMeasure == id)
        .Include(m => m.MeasureType)
        .Include(m => m.Vegetation)
        .Include(m => m.Worker)
        .Include(m => m.Vegetation.PlantClass)
        .Include(m => m.Worker.WorkerType)
        .SingleOrDefaultAsync();
      if (measure == null) {
        return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid measure id: {id}");
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
            WorkerTypeId = measure.Worker.WorkerTypeId,
            WorkerTypeCaption = measure.Worker.WorkerType != null ? measure.Worker.WorkerType.Caption : null
          }
        };
        return model;
      }
    }

    /// <summary>
    /// Delete a measure by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id) {
      var measure = await ctx.Measures.FindAsync(id);

      if (measure == null) {
        return NotFound();
      } else {
        ctx.Remove(measure);
        await ctx.SaveChangesAsync();
        return NoContent();
      }
    }

    /// <summary>
    /// Edit an existing measure
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model">Measure data, IDs must match</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, MeasureViewModel model) {
        if (model.IdMeasure != id)
        {
          return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Different ids {id} vs {model.IdMeasure}");
        } else {
          var measure = await ctx.Measures.FindAsync(id);
          if (measure == null) {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid measure id: {id}");
          }

          measure.PerformedOn = model.PerformedOn;
          measure.Description = model.Description;
          measure.MeasureTypeId = model.MeasureTypeId;
          measure.VegetationId = model.VegetationId;
          measure.WorkerId = model.WorkerId;
          measure.DurationMinutes = model.DurationMinutes;

          await ctx.SaveChangesAsync();
          return NoContent();
        }
    }

    /// <summary>
    /// Creates a new measure
    /// </summary>
    /// <param name="model">Measure data; Only perform datetime, measureTypeId and vegetationId are required</param>
    /// <returns></returns>
    [HttpPost()]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(MeasureViewModel model) {
      var measure = new Measure 
      {
        PerformedOn = model.PerformedOn,
        Description = model.Description,
        MeasureTypeId = model.MeasureTypeId,
        VegetationId = model.VegetationId,
        WorkerId = model.WorkerId,
        DurationMinutes = model.DurationMinutes
      };

      ctx.Add(measure);
      await ctx.SaveChangesAsync();

      var newMeasure = await Get(measure.IdMeasure);

      return CreatedAtAction(nameof(Get), new { id = measure.IdMeasure }, newMeasure.Value);
    }
  }
}