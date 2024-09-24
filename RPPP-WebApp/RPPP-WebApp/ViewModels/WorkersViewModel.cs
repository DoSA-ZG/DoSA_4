namespace RPPP_WebApp.ViewModels;

public class WorkersViewModel
{
  public IEnumerable<WorkerViewModel> Workers { get; set; }
  public PagingInfo PagingInfo { get; set; }
}