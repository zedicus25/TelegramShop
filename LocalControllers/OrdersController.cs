using TelegramShop.Models;

namespace TelegramShop.LocalControllers;

public class OrdersController
{
    public void AddOrder(Order order)
    {
        using (GamesShopContext context = new GamesShopContext())
        {
            context.Orders.Add(order);
            context.SaveChanges();
        }
    }

    public void MoveOrderToComplete(int id)
    {
        using (GamesShopContext context = new GamesShopContext())
        {
            var order = context.Orders.FirstOrDefault(x => x.Id == id);
            order.OrderStatusId = 2;
            context.SaveChanges();
        }
    }

    public List<Order> GetCompleteOrders() => new GamesShopContext().Orders.Where(x => x.OrderStatusId == 2).ToList();
    
    public List<Order> GetInProcessOrders() => new GamesShopContext().Orders.Where(x => x.OrderStatusId == 1).ToList();
}