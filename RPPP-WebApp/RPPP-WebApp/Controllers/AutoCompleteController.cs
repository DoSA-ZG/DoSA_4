using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class AutoCompleteController : Controller
{
  private readonly Rppp15Context ctx;
  private readonly AppSettings appData;

  public AutoCompleteController(Rppp15Context ctx, IOptionsSnapshot<AppSettings> options)
  {
    this.ctx = ctx;
    appData = options.Value;
  }

  public async Task<List<IdLabel>> WorkerTypes(string term)
  {
    var query = ctx.WorkerTypes
                    .Select(t => new IdLabel
                    {
                      Id = t.IdWorkerType,
                      Label = t.Caption
                    })
                    .Where(l => l.Label.Contains(term));

    var list = await query.OrderBy(l => l.Label)
                          .ThenBy(l => l.Id)
                          .Take(appData.AutoCompleteCount)
                          .ToListAsync();
    return list;
  }

  public async Task<List<IdLabel>> MeasureTypes(string term)
  {
    var query = ctx.MeasureTypes
                    .Select(t => new IdLabel
                    {
                      Id = t.IdMeasureType,
                      Label = t.Caption
                    })
                    .Where(l => l.Label.Contains(term));

    var list = await query.OrderBy(l => l.Label)
                          .ThenBy(l => l.Id)
                          .Take(appData.AutoCompleteCount)
                          .ToListAsync();
    return list;
  }
}