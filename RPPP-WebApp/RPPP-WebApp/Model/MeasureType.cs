using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class MeasureType
{
  public int IdMeasureType { get; set; }

  public string Caption { get; set; }

  public string Description { get; set; }

  public virtual ICollection<Measure> Measures { get; set; } = new List<Measure>();
}
