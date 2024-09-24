using System.ComponentModel.DataAnnotations;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels;

public class MeasureViewModel
{
  public int IdMeasure { get; set; }

  [Required]
  [Display(Name = "Performed On")]
  public DateTime PerformedOn { get; set; }

  [StringLength(255)]
  [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "-")]
  public string Description { get; set; }

  [Required]
  [Display(Name = "Type")]
  public int MeasureTypeId { get; set; } = -1;

  [Required]
  [Display(Name = "Vegetation")]
  public int VegetationId { get; set; } = -1;

  [Display(Name = "Worker")]
  public int? WorkerId { get; set; }

  [Display(Name = "Duration, minutes")]
  [DisplayFormat(NullDisplayText = "-")]
  public int? DurationMinutes { get; set; }

  public string MeasureTypeCaption { get; set; }

  public VegetationViewModel Vegetation { get; set; }

  [DisplayFormat(NullDisplayText = "-")]
  public WorkerViewModel Worker { get; set; }

  public string Summary
  {
    get
    {
      return MeasureTypeCaption + " on " + PerformedOn.ToString();
    }
  }
}