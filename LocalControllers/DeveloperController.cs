using TelegramShop.Data;
using TelegramShop.Models;

namespace TelegramShop.LocalControllers;

public class DeveloperController
{
    private GamesShopContext _dbContext;

    public DeveloperController(GamesShopContext context)
    {
        _dbContext = context;
    }
    public void AddDeveloper(Developer developer)
    {
      
            _dbContext.Developers.Add(developer);
            _dbContext.SaveChanges();
    }

    public void RemoveDeveloper(int id)
    {
      
            var dev = _dbContext.Developers.FirstOrDefault(x => x.Id == id);
            if (dev != null)
            {
                var games = _dbContext.Games.Where(x => x.DeveloperId == dev.Id);
                _dbContext.Games.RemoveRange(games);
                _dbContext.Developers.Remove(dev);
                _dbContext.SaveChanges();
            }
    }

    public List<Developer> GetAll() => _dbContext.Developers.ToList();
}