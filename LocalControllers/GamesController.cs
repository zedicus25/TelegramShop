using TelegramShop.Models;

namespace TelegramShop.LocalControllers;

public class GamesController
{
    public void AddGame(Game game)
    {
        using (GamesShopContext context = new GamesShopContext())
        {
            context.Games.Add(game);
            context.SaveChanges();
        }
    }
    public void RemoveGame(int id)
    {
        using (GamesShopContext context = new GamesShopContext())
        {
            var game = context.Games.FirstOrDefault(x => x.Id == id);
            if (game == null)
                return;
            context.Games.Remove(game);
            context.SaveChanges();
        }
    }

    public List<Game> GetAllGames() => new GamesShopContext().Games.ToList();

    public Game GetGameByName(string name) => new GamesShopContext().Games.FirstOrDefault(x => x.Name == name);

    public List<Game> GetGamesByCategory(int categoryId) =>
        new GamesShopContext().Games.Where(x => x.CategoryId == categoryId).ToList();

    public List<Game> GetGamesByDeveloper(int developerId) =>
        new GamesShopContext().Games.Where(x => x.DeveloperId == developerId).ToList();

}