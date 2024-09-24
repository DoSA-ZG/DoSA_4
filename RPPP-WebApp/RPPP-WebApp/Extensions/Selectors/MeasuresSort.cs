using RPPP_WebApp.Model;

namespace RPPP_WebApp.Extensions.Selectors;

public static class MeasuresSort
{
  public static IQueryable<Measure> ApplySort(this IQueryable<Measure> query, int sort, bool ascending)
  {
    System.Linq.Expressions.Expression<Func<Measure, object>> orderSelector = null;
    switch (sort)
    {
      case 1:
        orderSelector = m => m.IdMeasure;
        break;
      case 2:
        orderSelector = m => m.PerformedOn;
        break;
      case 3:
        orderSelector = m => m.Description;
        break;
      case 4:
        orderSelector = m => m.MeasureTypeId;
        break;
      case 5:
        orderSelector = m => m.VegetationId;
        break;
      case 6:
        orderSelector = m => m.WorkerId;
        break;
      case 7:
        orderSelector = m => m.DurationMinutes;
        break;
    }
    if (orderSelector != null)
    {
      query = ascending ?
             query.OrderBy(orderSelector) :
             query.OrderByDescending(orderSelector);
    }

    return query;
  }
}