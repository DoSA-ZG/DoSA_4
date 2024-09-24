using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class WorkerType
{
  public string Caption { get; set; }

  public string Description { get; set; }

  public int IdWorkerType { get; set; }

  public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
}
