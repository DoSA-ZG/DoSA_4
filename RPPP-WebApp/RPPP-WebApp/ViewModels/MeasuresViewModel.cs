namespace RPPP_WebApp.ViewModels;

public class MeasuresViewModel
{
  public IEnumerable<MeasureViewModel> Measures { get; set; }
  public PagingInfo PagingInfo { get; set; }
}