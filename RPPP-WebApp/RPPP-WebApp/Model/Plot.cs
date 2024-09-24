using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Plot
{
  public decimal? Latitude { get; set; }

  public decimal? Longitude { get; set; }

  public string Tag { get; set; }

  public double? Area { get; set; }

  public int IdPlot { get; set; }

  public string SoilQuality { get; set; }

  public string SoilType { get; set; }

  public string Sunlight { get; set; }

  public string Infrastructure { get; set; }

  public int? SubsidyId { get; set; }

  public int? LeaseId { get; set; }

  public string InsertIntoRppp15DboPlot { get; set; }

  public virtual Infrastructure InfrastructureNavigation { get; set; }

  public virtual Lease Lease { get; set; }

  public virtual SoilQuality SoilQualityNavigation { get; set; }

  public virtual SoilType SoilTypeNavigation { get; set; }

  public virtual Subsidy Subsidy { get; set; }

  public virtual Sunlight SunlightNavigation { get; set; }

  public virtual ICollection<Vegetation> Vegetations { get; set; } = new List<Vegetation>();
}
