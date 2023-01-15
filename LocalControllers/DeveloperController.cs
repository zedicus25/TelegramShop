using TelegramShop.Models;

namespace TelegramShop.LocalControllers;

public class DeveloperController
{
    public void AddDeveloper(Developer developer)
    {
        using (GamesShopContext context = new GamesShopContext())
        {
            context.Developers.Add(developer);
            context.SaveChanges();
        }
    }

    public void RemoveDeveloper(int id)
    {
        using (GamesShopContext context = new GamesShopContext())
        {
            var dev = context.Developers.FirstOrDefault(x => x.Id == id);
            if (dev != null)
            {
                var games = context.Games.Where(x => x.DeveloperId == dev.Id);
                context.Games.RemoveRange(games);
                context.Developers.Remove(dev);
                context.SaveChanges();
            }
        }
    }

    public List<Developer> GetAll() => new GamesShopContext().Developers.ToList();
}