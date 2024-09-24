using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Cuisine
{
  public string Caption { get; set; }

  public string Description { get; set; }

  public int IdCuisine { get; set; }

  public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}
