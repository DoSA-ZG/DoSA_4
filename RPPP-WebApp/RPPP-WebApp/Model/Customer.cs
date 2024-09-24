using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Customer
{
  public int IdCustomer { get; set; }

  public string Address { get; set; }

  public string Phone { get; set; }

  public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
