using System;
using System.Collections.Generic;

namespace NewVPP.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    public string? Note { get; set; }

    public bool? Deleted { get; set; }

    public int? TransactStatusId { get; set; }

    public string? Address { get; set; }

    public int? TotalMoney { get; set; }
    public int? TypePayment { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual TransactStatus? TransactStatus { get; set; }
}
