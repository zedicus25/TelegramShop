using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramShop.Data;
using TelegramShop.Models;

namespace TelegramShop.LocalControllers;

public class TgControllerClient
{
    private const string DEFAULT_GAME_ICON = "https://cdn-icons-png.flaticon.com/512/3408/3408506.png";
    
    private GamesShopContext _dbContext;

    public TgControllerClient(GamesShopContext context)
    {
        _dbContext = context;
    }
    
    public async void GetAllGames(long id)
    {
        var categories = new CategoriesController(_dbContext).GetAllCategories();
        var developers = new DeveloperController(_dbContext).GetAll();
        foreach (var game in new GamesController(_dbContext).GetAllGames())
        {
            var category = categories.FirstOrDefault(x => x.Id == game.CategoryId);
            var developer = developers.FirstOrDefault(x => x.Id == game.DeveloperId);
            var str = $"Title: {game.Name}\nDescription: {game.Description}\nDeveloper: {developer.Name}" +
                      $"\nCategory: {category.Title}\nPrice: {game.Price}₴";
            await TgBotClient.Instance.Client.SendPhotoAsync(id,
                photo: game.PhotoLink ?? DEFAULT_GAME_ICON,
                caption: str,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData("Order", "orderGame")));
        }
    }

    public async void GetAllCategories(long id)
    {
        foreach (var category in new CategoriesController(_dbContext).GetAllCategories())
        {
            var str = $"Title: {category.Title}\nDescription: {category.Description}";
            await TgBotClient.Instance.Client.SendTextMessageAsync(id, str);
        }
    }

    public async void GetAllDevelopers(long id)
    {
        foreach (var category in new DeveloperController(_dbContext).GetAll())
        {
            var str = $"Name: {category.Name}\nDescription: {category.Description}";
            await TgBotClient.Instance.Client.SendTextMessageAsync(id, str);
        }
    }

    public async void OrderGame(Update update)
    {
        var strings = update.CallbackQuery.Message.Caption.Split('\n');
        var name = strings.FirstOrDefault(x => x.Contains("Title:")).
            Substring(strings.FirstOrDefault(x => x.Contains("Title:")).IndexOf(':')+1).Trim();

        int gameId = new GamesController(_dbContext).GetGameByName(name).Id;
        int userId = new UsersController(_dbContext).GetAllUsers().FirstOrDefault(x => x.TgId == update.CallbackQuery.From.Id).Id;
        
        Order order = new Order()
        {
            GameId = gameId,
            UserId = userId,
            OrderDate = DateTime.Now,
            OrderStatusId = 1
        };
        new OrdersController(_dbContext).AddOrder(order);
        await TgBotClient.Instance.Client.SendTextMessageAsync(update.CallbackQuery.From.Id, "You order game");
    }

    public async void GetGameByName(string msg, long id)
    {
        var game = new GamesController(_dbContext).GetGameByName(msg);
        var category = new CategoriesController(_dbContext).GetAllCategories().FirstOrDefault(x => x.Id == game.CategoryId);
        var developer = new DeveloperController(_dbContext).GetAll().FirstOrDefault(x => x.Id == game.DeveloperId);
        var str = $"Title: {game.Name}\nDescription: {game.Description}\nDeveloper: {developer.Name}\n" +
                  $"Category: {category.Title}Price: {game.Price}₴";
        await TgBotClient.Instance.Client.SendPhotoAsync(id,
            photo: game.PhotoLink ?? DEFAULT_GAME_ICON,
            caption: str,
            replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Order", "orderGame")));
    }

    public async void GetGamesByCategory(string msg, long id)
    {
        int categoryId = new CategoriesController(_dbContext).GetAllCategories()
            .FirstOrDefault(x => x.Title == msg).Id;
        if (categoryId == 0)
            await TgBotClient.Instance.Client.SendTextMessageAsync(id,
                "Cannot find this category");
        var games = new GamesController(_dbContext).GetGamesByCategory(categoryId);
        var categories = new CategoriesController(_dbContext).GetAllCategories();
        var developers = new DeveloperController(_dbContext).GetAll();
        foreach (var game in games)
        {
            var category = categories.FirstOrDefault(x => x.Id == game.CategoryId);
            var developer = developers.FirstOrDefault(x => x.Id == game.DeveloperId);
            var str = $"Title: {game.Name}\nDescription: {game.Description}\nDeveloper: {developer.Name}\n" +
                      $"Category: {category.Title}\nPrice: {game.Price}₴";
            await TgBotClient.Instance.Client.SendPhotoAsync(id,
                photo: game.PhotoLink ?? DEFAULT_GAME_ICON,
                caption: str,
                replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Order", "orderGame")));
        }
    }

    public async void GetGamesDeveloper(string msg, long id)
    {
        int developerId = new DeveloperController(_dbContext).GetAll()
            .FirstOrDefault(x => x.Name == msg).Id;
        if (developerId == 0)
            await TgBotClient.Instance.Client.SendTextMessageAsync(id,
                "Cannot find this developer");
        var games = new GamesController(_dbContext).GetGamesByDeveloper(developerId);
        var categories = new CategoriesController(_dbContext).GetAllCategories();
        var developers = new DeveloperController(_dbContext).GetAll();
        foreach (var game in games)
        {
            var category = categories.FirstOrDefault(x => x.Id == game.CategoryId);
            var developer = developers.FirstOrDefault(x => x.Id == game.DeveloperId);
            var str = $"Title: {game.Name}\nDescription: {game.Description}\nDeveloper: {developer.Name}\n" +
                      $"Category: {category.Title}\nPrice: {game.Price}₴";
            await TgBotClient.Instance.Client.SendPhotoAsync(id,
                photo: game.PhotoLink ?? DEFAULT_GAME_ICON,
                caption: str,
                replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Order", "orderGame")));
        }
    }
}