using System;
using System.Collections.Generic;

namespace TelegramShop.Models;

public partial class Developer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? PhotoLink { get; set; }

    public virtual ICollection<Game> Games { get; } = new List<Game>();
}
