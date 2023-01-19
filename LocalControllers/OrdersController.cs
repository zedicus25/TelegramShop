using TelegramShop.Data;
using TelegramShop.Models;

namespace TelegramShop.LocalControllers;

public class OrdersController
{
    private GamesShopContext _dbContext;

    public OrdersController(GamesShopContext context)
    {
        _dbContext = context;
    }

    public void AddOrder(Order order)
    {
        _dbContext.Orders.Add(order);
        _dbContext.SaveChanges();
    }

    public void MoveOrderToComplete(int id)
    {
        var order = _dbContext.Orders.FirstOrDefault(x => x.Id == id);
        order.OrderStatusId = 2;
        _dbContext.SaveChanges();
    }

    public List<Order> GetCompleteOrders() => _dbContext.Orders.Where(x => x.OrderStatusId == 2).ToList();

    public List<Order> GetInProcessOrders() => _dbContext.Orders.Where(x => x.OrderStatusId == 1).ToList();
}