using System;
using System.Collections.Generic;

namespace TelegramShop.Models;

public partial class User
{
    public int Id { get; set; }

    public long? TgId { get; set; }

    public string? TgUserName { get; set; }

    public string? LastName { get; set; }

    public string FirstName { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; } = new List<Order>();
}
