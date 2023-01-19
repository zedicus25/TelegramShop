using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramShop.Data;
using TelegramShop.Models;
using Game = TelegramShop.Models.Game;

namespace TelegramShop.LocalControllers;

public class TgControllerAdmin
{
    private const string DEFAULT_DEVELOPER_ICON = "https://miro.medium.com/max/1400/1*KbYaeKKyTGzNTKjIiBzb3w.png";
    private const string DEFAULT_GAME_ICON = "https://cdn-icons-png.flaticon.com/512/3408/3408506.png";
    private GamesShopContext _dbContext;

    public TgControllerAdmin(GamesShopContext context)
    {
        _dbContext = context;
    }

    public async void GetAllClients(long id)
    {
        foreach (var user in new UsersController(_dbContext).GetAllUsers())
        {
            var str = $"Id: {user.Id}\nTg id: {user.TgId}\nUser name: {user.TgUserName}" +
                      $"\nFirst name: {user.FirstName}\nLast name: {user.LastName}";
            await TgBotAdmin.Instance.Client.SendTextMessageAsync(id,
                str,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData("Send message", "sendMessageToClient")));
        }
    }

    public async void MoveOrderToHistory(Update update)
    {
        var strings = update.CallbackQuery.Message.Text.Split('\n');
        var id = Convert.ToInt32(strings.FirstOrDefault(x => x.Contains("Id:")).
            Substring(strings.FirstOrDefault(x => x.Contains("Id:")).IndexOf(':')+1).Trim());
        new OrdersController(_dbContext).MoveOrderToComplete(id);
        await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.CallbackQuery.From.Id,
            "Order was moved to history");
    }

    public async void GetHistoryData(long id)
    {
        foreach (var order in new OrdersController(_dbContext).GetCompleteOrders())
        {
            string str = $"Id: {order.Id}\nGame Id: {order.GameId}\nUser id: {order.UserId}\nOrder date: {order.OrderDate}\n" +
                         $"Orders status id: {order.OrderStatusId}";
            await TgBotAdmin.Instance.Client.SendTextMessageAsync(id, str);
        }
    }

    public async void GetActiveOrders(long id)
    {
        foreach (var order in new OrdersController(_dbContext).GetInProcessOrders())
        {
            string str = $"Id: {order.Id}\nGame Id: {order.GameId}\nUser id: {order.UserId}\nOrder date: {order.OrderDate}\n" +
                         $"Orders status id: {order.OrderStatusId}";
            await TgBotAdmin.Instance.Client.SendTextMessageAsync(id, str, 
                replyMarkup:new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Move to history", "moveOrderToHistory")));
        }
    }

    public async void GetAllDevelopers(long id)
    {
        foreach (var category in new DeveloperController(_dbContext).GetAll())
        {
            var str = $"Id:{category.Id}\nTitle: {category.Name}\nDescription: {category.Description}";
            await TgBotAdmin.Instance.Client.SendPhotoAsync(id,
                photo: category.PhotoLink ?? DEFAULT_DEVELOPER_ICON,
                caption: str,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData("Remove", "removeDeveloper")));
        }
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
            await TgBotAdmin.Instance.Client.SendPhotoAsync(id,
                photo: game.PhotoLink ?? DEFAULT_GAME_ICON,
                caption: str,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData("Remove", "removeGame")));
        }
    }

    public async void GetAllCategories(long id)
    {
        foreach (var category in new CategoriesController(_dbContext).GetAllCategories())
        {
            var str = $"Id:{category.Id}\nTitle: {category.Title}\nDescription: {category.Description}";
            await TgBotAdmin.Instance.Client.SendTextMessageAsync(id,
                str,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData("Remove", "removeCategory")));
        }
    }

    public async void AddCategory(string msg, long id)
    {
        var props = msg.Split('\n');
        Category newCategory = new Category();
        foreach (var prop in props)
        {
            if (prop.Contains("Title:"))
                newCategory.Title = prop.Substring(prop.IndexOf(':') + 1).Trim();
            else if (prop.Contains("Description:"))
                newCategory.Description = prop.Substring(prop.IndexOf(':') + 1).Trim();
        }

        new CategoriesController(_dbContext).AddCategory(newCategory);
        await TgBotAdmin.Instance.Client.SendTextMessageAsync(id,
            "New category was added!");
    }

    public async void RemoveCategory(string updateMsg, long updateId)
    {
        if (updateMsg.Contains("Id:"))
        {
            try
            {
                int id = Convert.ToInt32(updateMsg.Substring(updateMsg.IndexOf(':')).Trim());
                if (id <= 0)
                    throw new ArgumentException("Id cannot be negative or zero!");
                new CategoriesController(_dbContext).RemoveCategory(id);
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(updateId,
                    "Category was removed! Games in this category also was removed!");
            }
            catch (Exception e)
            {
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(updateId,
                    "Incorrect format!");
            }
        }
    }

    public async void AddDeveloper(string msg, long id)
    {
        var props = msg.Split('\n');
        Developer newDeveloper = new Developer();
        foreach (var prop in props)
        {
            if (prop.Contains("Name:"))
                newDeveloper.Name = prop.Substring(prop.IndexOf(':') + 1).Trim();
            else if (prop.Contains("Description:"))
                newDeveloper.Description = prop.Substring(prop.IndexOf(':') + 1).Trim();
            else if (prop.Contains("PhotoLink:"))
                newDeveloper.PhotoLink = prop.Substring(prop.IndexOf(':') + 1).Trim();
        }

        new DeveloperController(_dbContext).AddDeveloper(newDeveloper);
        await TgBotAdmin.Instance.Client.SendTextMessageAsync(id,
            "New developer was added!");
    }

    public async void RemoveDeveloper(string updateMsg, long updateId)
    {
        if (updateMsg.Contains("Id:"))
        {
            try
            {
                int id = Convert.ToInt32(updateMsg.Substring(updateMsg.IndexOf(':')).Trim());
                if (id <= 0)
                    throw new ArgumentException("Id cannot be negative or zero!");
                new DeveloperController(_dbContext).RemoveDeveloper(id);
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(updateId,
                    "Developer was removed! Games this developers also removed!");
            }
            catch (Exception e)
            {
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(updateId,
                    "Incorrect format!");
            }
        }
    }

    public async void AddGame(string msg, long id)
    {
        var props = msg.Split('\n');
        Game newGame = new Game();
        try
        {
            foreach (var prop in props)
            {
                if (prop.Contains("Name:"))
                    newGame.Name = prop.Substring(prop.IndexOf(':') + 1).Trim();
                else if (prop.Contains("Description:"))
                    newGame.Description = prop.Substring(prop.IndexOf(':') + 1).Trim();
                else if (prop.Contains("PhotoLink:"))
                    newGame.PhotoLink = prop.Substring(prop.IndexOf(':') + 1).Trim();
                else if (prop.Contains("Price:"))
                    newGame.Price = Convert.ToSingle(prop.Substring(prop.IndexOf(':') + 1).Trim());
                else if (prop.Contains("DeveloperId:"))
                    newGame.DeveloperId = Convert.ToInt32(prop.Substring(prop.IndexOf(':') + 1).Trim());
                else if (prop.Contains("CategoryId:"))
                    newGame.CategoryId = Convert.ToInt32(prop.Substring(prop.IndexOf(':') + 1).Trim());
            }
        }
        catch (Exception e)
        {
            await TgBotAdmin.Instance.Client.SendTextMessageAsync(id,
                "Incorrect format!");
        }

        new GamesController(_dbContext).AddGame(newGame);
        await TgBotAdmin.Instance.Client.SendTextMessageAsync(id,
            "New game was added!");
    }

    public async void RemoveGame(string msg, long updateId)
    {
        if (msg.Contains("Id:"))
        {
            try
            {
                int id = Convert.ToInt32(msg.Substring(msg.IndexOf(':')).Trim());
                if (id <= 0)
                    throw new ArgumentException("Id cannot be negative or zero!");
                new GamesController(_dbContext).RemoveGame(id);
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(updateId,
                    "Game was removed!");
            }
            catch (Exception e)
            {
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(updateId,
                    "Incorrect format!");
            }
        }
    }
}