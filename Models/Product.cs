using System;
using System.Collections.Generic;

namespace NewVPP.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public int? CatId { get; set; }

    public string? ProductName { get; set; }

    public string? Description { get; set; }

    public int? Price { get; set; }

    public string? Thumb { get; set; }

    public DateTime? DateCreate { get; set; }

    public virtual Category? Cat { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
