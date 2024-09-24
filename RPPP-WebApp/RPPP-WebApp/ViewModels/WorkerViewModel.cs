using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RPPP_WebApp.ViewModels;

public class WorkerViewModel
{
  [Display(Name = "Id")]
  public int IdWorker { get; set; }

  [Required]
  [DataType(DataType.Currency)]
  [Display(Name = "Daily Wage")]
  [Range(0, (double)decimal.MaxValue)]
  public decimal DailyWage { get; set; }

  [StringLength(127)]
  [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "-")]
  public string Tag { get; set; }

  [StringLength(255)]
  public string Notes { get; set; }

  [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "-")]
  [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$")]
  public string Email { get; set; }

  [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "-")]
  [RegularExpression(@"[\d+\-()\s]*"), StringLength(255)]
  public string Phone { get; set; }

  [Display(Name = "Type")]
  public int? WorkerTypeId { get; set; }
  [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "-")]
  public string WorkerTypeCaption { get; set; }

  public virtual ICollection<MeasureViewModel> Measures { get; set; } = new List<MeasureViewModel>();

  [ValidateNever]
  [Display(Name = "Measures")]
  [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "-")]
  public string MeasuresSummary
  {
    get
    {
      string summary = "";

      int i = 0;
      foreach (var measure in Measures)
      {
        summary += measure.Summary;
        if (i < Measures.Count - 1)
        {
          summary += ", ";
        }
        i++;
      }

      return summary;
    }
  }

  public string Summary
  {
    get
    {
      string summary = "#" + IdWorker;
      summary += " - paid " + DailyWage.ToString("C") + " daily";
      if (!string.IsNullOrEmpty(Tag)) {
        summary += ", tagged as " + Tag;
      }
      return summary;
    }
  }

  public WorkerViewModel()
  {
    Measures = new List<MeasureViewModel>();
  }
}