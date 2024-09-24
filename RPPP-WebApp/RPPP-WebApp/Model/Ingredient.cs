using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Model;

public partial class Ingredient
{
    public int IdIngredient { get; set; }

    public int PlantClassId { get; set; }

    public int RecipeId { get; set; }
    [NotMapped] // This property is not part of the database schema
    public string SelectedRecipe { get; set; }

    public virtual PlantClass PlantClass { get; set; }

    public virtual Recipe Recipe { get; set; }
}
