using System.ComponentModel.DataAnnotations;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels;

public class VegetationViewModel
{
  public IEnumerable<Vegetation> Vegetations { get; set; }
  public IEnumerable<Plot> AllPlots { get; set; }
  public IEnumerable<PlantClass> AllPlants { get; set; }
  public PagingInfo PagingInfo { get; set; } 
  public int IdVegetation { get; set; }

  public int Units { get; set; }
  
  public DateTime PlantedOn { get; set; }

  public DateTime? RemovedOn { get; set; }

  public DateTime? YieldAnticipatedOn { get; set; }

  public DateTime? ExpiryAnticipatedOn { get; set; }
  
  public string PlotTag { get; set; }
  public int PlotId { get; set; }

  public int PlantClassId { get; set; }

  public string PlantClassName { get; set; }

  public string Summary
  {
    get
    {
      string summary = "#" + IdVegetation;
      summary += " - " + Units + " units of " + PlantClassName;
      summary += " within plot #" + PlotId;
      return summary;
    }
  }
}