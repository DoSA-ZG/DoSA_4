using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Task
{
  public int IdTask { get; set; }

  public string Description { get; set; }

  public bool Completed { get; set; }

  public DateTime? Deadline { get; set; }
}
