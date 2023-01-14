using System;
using System.Collections.Generic;

namespace TelegramShop.Models;

public partial class OrdersStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; } = new List<Order>();
}
