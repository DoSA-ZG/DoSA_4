using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Lease
{
  public int IdLease { get; set; }

  public decimal YearlyPay { get; set; }

  public DateTime? ExpiresOn { get; set; }

  public string Link { get; set; }

  public virtual ICollection<Plot> Plots { get; set; } = new List<Plot>();
}
