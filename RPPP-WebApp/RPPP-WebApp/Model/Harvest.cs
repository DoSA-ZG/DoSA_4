using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Model
{
    public partial class Harvest
    {
        public int IdHarvest { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH\\:mm\\:ss}", ApplyFormatInEditMode = true)]
        [Range(typeof(DateTime), "1900-01-01T00:00:00", "2024-12-31T23:59:59", ErrorMessage = "Invalid date selected!")]
        public DateTime CollectedOn { get; set; }



        [Range(0.1, double.MaxValue, ErrorMessage = "Weight must be greater than 0")]
        public double Weight { get; set; }

        public string Tag { get; set; }

        [NotMapped] // This property is not part of the database schema
        public string SelectedPlantClassName { get; set; }

        public int VegetationId { get; set; }

        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

        public virtual Vegetation Vegetation { get; set; }
    }
}
