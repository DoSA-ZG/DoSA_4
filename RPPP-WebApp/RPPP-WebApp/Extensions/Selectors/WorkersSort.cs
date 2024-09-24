using RPPP_WebApp.Model;

namespace RPPP_WebApp.Extensions.Selectors;

public static class WorkersSort
{
  public static IQueryable<Worker> ApplySort(this IQueryable<Worker> query, int sort, bool ascending)
  {
    System.Linq.Expressions.Expression<Func<Worker, object>> orderSelector = null;
    switch (sort)
    {
      case 1:
        orderSelector = w => w.IdWorker;
        break;
      case 2:
        orderSelector = w => w.WorkerTypeId;
        break;
      case 3:
        orderSelector = w => w.Tag;
        break;
      case 4:
        orderSelector = w => w.DailyWage;
        break;
      case 5:
        orderSelector = w => w.Email;
        break;
      case 6:
        orderSelector = w => w.Phone;
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
