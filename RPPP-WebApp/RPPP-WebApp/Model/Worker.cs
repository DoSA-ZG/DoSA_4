using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Worker
{
  public int IdWorker { get; set; }

  public decimal DailyWage { get; set; }

  public string Tag { get; set; }

  public string Notes { get; set; }

  public int? WorkerTypeId { get; set; }

  public string Email { get; set; }

  public string Phone { get; set; }

  public virtual ICollection<Measure> Measures { get; set; } = new List<Measure>();

  public virtual WorkerType WorkerType { get; set; }
}
