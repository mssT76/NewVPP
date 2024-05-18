using System;
using System.Collections.Generic;

namespace NewVPP.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public DateTime? Birthday { get; set; }

    public string? Password { get; set; }

    public string? Address { get; set; }

    public int? LocationId { get; set; }

    public virtual Location? Location { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
