using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Order
{
  public int IdOrder { get; set; }

  public DateTime IssuedOn { get; set; }

  public string Description { get; set; }

  public int CustomerId { get; set; }

  public virtual Customer Customer { get; set; }

  public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
