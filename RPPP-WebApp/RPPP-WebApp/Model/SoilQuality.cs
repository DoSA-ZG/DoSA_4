using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class SoilQuality
{
  public string Caption { get; set; }

  public string Description { get; set; }

  public virtual ICollection<Plot> Plots { get; set; } = new List<Plot>();
}
