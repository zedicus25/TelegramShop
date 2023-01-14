using System;
using System.Collections.Generic;

namespace TelegramShop.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<Game> Games { get; } = new List<Game>();
}
