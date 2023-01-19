using TelegramShop.Data;
using TelegramShop.Models;

namespace TelegramShop.LocalControllers;

public class GamesController
{
    private GamesShopContext _dbContext;

    public GamesController(GamesShopContext context)
    {
        _dbContext = context;
    }

    public void AddGame(Game game)
    {
        _dbContext.Games.Add(game);
        _dbContext.SaveChanges();
    }

    public void RemoveGame(int id)
    {
        var game = _dbContext.Games.FirstOrDefault(x => x.Id == id);
        if (game == null)
            return;
        _dbContext.Games.Remove(game);
        _dbContext.SaveChanges();
    }

    public List<Game> GetAllGames() => _dbContext.Games.ToList();

    public Game GetGameByName(string name) => _dbContext.Games.FirstOrDefault(x => x.Name == name);

    public List<Game> GetGamesByCategory(int categoryId) =>
        _dbContext.Games.Where(x => x.CategoryId == categoryId).ToList();

    public List<Game> GetGamesByDeveloper(int developerId) =>
        _dbContext.Games.Where(x => x.DeveloperId == developerId).ToList();
}