using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Subsidy
{
  public int IdSubsidy { get; set; }

  public decimal Amount { get; set; }

  public DateTime? ExpiresOn { get; set; }

  public virtual ICollection<Plot> Plots { get; set; } = new List<Plot>();
}
