using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Model;

public partial class PlantClass
{
    public int IdPlantClass { get; set; }

    public string Name { get; set; }

    public string Passport { get; set; }

    public int? ParentId { get; set; }
    [Range(1, double.MaxValue, ErrorMessage = "Fiber per serving must be greater than zero.")]
    public double? FiberPerServing { get; set; }
    [Range(1, double.MaxValue, ErrorMessage = "Potassium per serving must be greater than zero.")]
    public double? PotassiumPerServing { get; set; }


    public virtual ICollection<Ingredient> Ingredients { get; } = new List<Ingredient>();

    public virtual ICollection<PlantClass> InverseParent { get; } = new List<PlantClass>();

    public virtual PlantClass Parent { get; set; }

    public virtual ICollection<Vegetation> Vegetations { get; } = new List<Vegetation>();
}
