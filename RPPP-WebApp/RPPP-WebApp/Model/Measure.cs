using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Measure
{
  public int IdMeasure { get; set; }

  public DateTime PerformedOn { get; set; }

  public string Description { get; set; }

  public int MeasureTypeId { get; set; }

  public int VegetationId { get; set; }

  public int? WorkerId { get; set; }

  public int? DurationMinutes { get; set; }

  public virtual MeasureType MeasureType { get; set; }

  public virtual Vegetation Vegetation { get; set; }

  public virtual Worker Worker { get; set; }
}
