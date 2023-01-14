using System;
using System.Collections.Generic;

namespace TelegramShop.Models;

public partial class Game
{
    public int Id { get; set; }

    public int? CategoryId { get; set; }

    public int? DeveloperId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public double Price { get; set; }

    public string? PhotoLink { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Developer? Developer { get; set; }

    public virtual ICollection<Order> Orders { get; } = new List<Order>();
}
