using System.ComponentModel.DataAnnotations;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels;

public class MeasureReportViewModel
{
  public int IdMeasure { get; set; }

  public DateTime PerformedOn { get; set; }

  public string Description { get; set; }

  public int MeasureTypeId { get; set; } = -1;

  public int VegetationId { get; set; } = -1;

  public int? WorkerId { get; set; }

  public int? DurationMinutes { get; set; }

  public string MeasureTypeCaption { get; set; }

  public string VegetationSummary { get; set; }

  public decimal WorkerSalary { get; set; }

  public decimal WorkerWage { get; set; }
}