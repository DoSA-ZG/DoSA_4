using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Model;

public partial class Purchase
{
  public int IdPurchase { get; set; }
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH\\:mm\\:ss}", ApplyFormatInEditMode = true)]
    [Range(typeof(DateTime), "1900-01-01T00:00:00", "2024-12-31T23:59:59", ErrorMessage = "Invalid date selected!")]
    public DateTime CollectedOn { get; set; }


    [Range(0.1, double.MaxValue, ErrorMessage = "Weight must be greater than 0")]
    public double Weight { get; set; }

    [Range(0.1, double.MaxValue, ErrorMessage = "Gain must be greater than 0")]
    public decimal Gain { get; set; }

    public string Tag { get; set; }

  public int OrderId { get; set; }

  public int HarvestId { get; set; }

  public virtual Harvest Harvest { get; set; }

  public virtual Order Order { get; set; }
}
