using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class ClassUsability
{
  public int PlantClassId { get; set; }

  public int UsabilityId { get; set; }

  public virtual PlantClass PlantClass { get; set; }

  public virtual Usability Usability { get; set; }
}
