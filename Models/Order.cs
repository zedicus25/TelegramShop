using System;
using System.Collections.Generic;

namespace TelegramShop.Models;

public partial class Order
{
    public int Id { get; set; }

    public int? GameId { get; set; }

    public int? UserId { get; set; }

    public DateTime OrderDate { get; set; }

    public int? OrderStatusId { get; set; }

    public virtual Game? Game { get; set; }

    public virtual OrdersStatus? OrderStatus { get; set; }

    public virtual User? User { get; set; }
}
