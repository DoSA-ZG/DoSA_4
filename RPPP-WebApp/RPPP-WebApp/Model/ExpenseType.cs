using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class ExpenseType
{
  public int IdExpenseType { get; set; }

  public string Caption { get; set; }

  public string Description { get; set; }

  public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
