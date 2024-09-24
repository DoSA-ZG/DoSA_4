using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Vegetation
{
  public int IdVegetation { get; set; }

  public int Units { get; set; }

  public DateTime PlantedOn { get; set; }

  public DateTime? RemovedOn { get; set; }

  public DateTime? YieldAnticipatedOn { get; set; }

  public DateTime? ExpiryAnticipatedOn { get; set; }

  public int PlotId { get; set; }

  public int PlantClassId { get; set; }

  public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

  public virtual ICollection<Harvest> Harvests { get; set; } = new List<Harvest>();

  public virtual ICollection<Measure> Measures { get; set; } = new List<Measure>();

  public virtual PlantClass PlantClass { get; set; }

  public virtual Plot Plot { get; set; }
}
