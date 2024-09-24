using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Model;

public partial class Recipe
{
    public int IdRecipe { get; set; }

    public string Description { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "Calories per serving must be greater than zero.")]
    public double CaloriesPerServing { get; set; }
    [Range(1, double.MaxValue, ErrorMessage = "Duration must be greater than zero.")]
    public double? ApproximateDuration { get; set; }

    public int? CuisineId { get; set; }

    public string Caption { get; set; }

    public virtual Cuisine Cuisine { get; set; }

    public virtual ICollection<Ingredient> Ingredients { get; } = new List<Ingredient>();
}
