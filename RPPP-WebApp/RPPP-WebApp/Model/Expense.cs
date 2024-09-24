using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Expense
{
  public int IdExpense { get; set; }

  public DateTime PaidOn { get; set; }

  public decimal Amount { get; set; }

  public string Description { get; set; }

  public int ExpenseTypeId { get; set; }

  public int VegetationId { get; set; }

  public virtual ExpenseType ExpenseType { get; set; }

  public virtual Vegetation Vegetation { get; set; }
}
