using System.Text;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramShop.Data;
using TelegramShop.LocalControllers;
using TelegramShop.Models;
using Game = TelegramShop.Models.Game;
using User = TelegramShop.Models.User;

namespace TelegramShop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TgController: ControllerBase
{
    private readonly GamesShopContext _dbContext;
    
    private const string DEFAULT_GAME_ICON = "https://cdn-icons-png.flaticon.com/512/3408/3408506.png";
    private const string DEFAULT_DEVELOPER_ICON = "https://miro.medium.com/max/1400/1*KbYaeKKyTGzNTKjIiBzb3w.png";
    private readonly InlineKeyboardMarkup _categoryInlineMarkup;
    private readonly InlineKeyboardMarkup _gamesInlineMarkup;
    private readonly InlineKeyboardMarkup _developersInlineMarkup;
    private readonly InlineKeyboardMarkup _ordersInlineMarkup;
    private static bool _isAddingCategory;
    private static bool _isRemovingCategory;
    private static bool _isAddingGame;
    private static bool _isRemovingGame;
    private static bool _isAddingDeveloper;
    private static bool _isRemovingDeveloper;
    private static bool _gettingGameByName;
    private static bool _sendingMsgToClient;
    private static bool _gettingGamesByCategory;
    private static bool _gettingGamesByDeveloper;
    private static long _clientIdForSending;
    private static bool _isSendingMessageToAll;

    public TgController(GamesShopContext context)
    {
        InlineKeyboardButton[] categoryInlineButtons = new[]
        {
            InlineKeyboardButton.WithCallbackData("Add", "addCategory"),
            InlineKeyboardButton.WithCallbackData("Remove", "removeCategory"),
            InlineKeyboardButton.WithCallbackData("Show all", "showCategories"),
        };
        InlineKeyboardButton[] gamesInlineButtons = new[]
        {
            InlineKeyboardButton.WithCallbackData("Add", "addGame"),
            InlineKeyboardButton.WithCallbackData("Remove", "removeGame"),
            InlineKeyboardButton.WithCallbackData("Show all", "showGames"),
        };
        InlineKeyboardButton[] developersInlineButtons = new[]
        {
            InlineKeyboardButton.WithCallbackData("Add", "addDeveloper"),
            InlineKeyboardButton.WithCallbackData("Remove", "removeDeveloper"),
            InlineKeyboardButton.WithCallbackData("Show all", "showDevelopers"),
        };
        InlineKeyboardButton[] orderInlineButtons = new[]
        {
            InlineKeyboardButton.WithCallbackData("Get active orders", "getActiveOrders"),
            InlineKeyboardButton.WithCallbackData("Get history orders", "getHistoryOrders"),
        };
        _categoryInlineMarkup = new InlineKeyboardMarkup(categoryInlineButtons);
        _gamesInlineMarkup = new InlineKeyboardMarkup(gamesInlineButtons);
        _developersInlineMarkup = new InlineKeyboardMarkup(developersInlineButtons);
        _ordersInlineMarkup = new InlineKeyboardMarkup(orderInlineButtons);
        _dbContext = context;
    }

    [HttpPost]
    public async Task<IResult> Post([FromBody] Update update)
    {
        if (update != null)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                {
                    if (update.Message.Text.StartsWith('/'))
                    {
                        await DoCommand(update);
                    }
                    else
                    {
                        await DoCommandAction(update);
                    }

                    break;
                }
                case UpdateType.CallbackQuery:
                    await DoCallBackQuery(update);
                    break;
            }
        }

        return Results.Ok();
    }

    private async Task DoCommandAction(Update update)
    {
        if (_isAddingCategory)
        {
            _isAddingCategory = false;
            var props = update.Message.Text.Split('\n');
            Category newCategory = new Category();
            foreach (var prop in props)
            {
                if (prop.Contains("Title:"))
                    newCategory.Title = prop.Substring(prop.IndexOf(':') + 1).Trim();
                else if (prop.Contains("Description:"))
                    newCategory.Description = prop.Substring(prop.IndexOf(':') + 1).Trim();
            }

            new CategoriesController(_dbContext).AddCategory(newCategory);
            await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
                "New category was added!");
        }
        else if (_isRemovingCategory)
        {
            _isRemovingCategory = false;
            if (update.Message.Text.Contains("Id:"))
            {
                string msg = update.Message.Text;
                try
                {
                    int id = Convert.ToInt32(msg.Substring(msg.IndexOf(':')).Trim());
                    if (id <= 0)
                        throw new ArgumentException("Id cannot be negative or zero!");
                    new CategoriesController(_dbContext).RemoveCategory(id);
                    await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
                        "Category was removed! Games in this category also was removed!");
                }
                catch (Exception e)
                {
                    await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
                        "Incorrect format!");
                }
            }
        }
        else if (_isAddingDeveloper)
        {
            _isAddingDeveloper = false;
            var props = update.Message.Text.Split('\n');
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
            await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
                "New developer was added!");
        }
        else if (_isRemovingDeveloper)
        {
            _isRemovingDeveloper = false;
            if (update.Message.Text.Contains("Id:"))
            {
                string msg = update.Message.Text;
                try
                {
                    int id = Convert.ToInt32(msg.Substring(msg.IndexOf(':')).Trim());
                    if (id <= 0)
                        throw new ArgumentException("Id cannot be negative or zero!");
                    new DeveloperController(_dbContext).RemoveDeveloper(id);
                    await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
                        "Developer was removed! Games this developers also removed!");
                }
                catch (Exception e)
                {
                    await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
                        "Incorrect format!");
                }
            }
        }
        else if (_isAddingGame)
        {
            _isAddingGame = false;
            var props = update.Message.Text.Split('\n');
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
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
                    "Incorrect format!");
            }

            new GamesController(_dbContext).AddGame(newGame);
            await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
                "New game was added!");
        }
        else if (_isRemovingGame)
        {
            _isRemovingGame = false;
            if (update.Message.Text.Contains("Id:"))
            {
                string msg = update.Message.Text;
                try
                {
                    int id = Convert.ToInt32(msg.Substring(msg.IndexOf(':')).Trim());
                    if (id <= 0)
                        throw new ArgumentException("Id cannot be negative or zero!");
                    new GamesController(_dbContext).RemoveGame(id);
                    await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
                        "Game was removed!");
                }
                catch (Exception e)
                {
                    await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
                        "Incorrect format!");
                }
            }
        }
        else if (_gettingGameByName)
        {
            _gettingGameByName = false;
            string msg = update.Message.Text.Trim();
            var game = new GamesController(_dbContext).GetGameByName(msg);
            var category = new CategoriesController(_dbContext).GetAllCategories().FirstOrDefault(x => x.Id == game.CategoryId);
            var developer = new DeveloperController(_dbContext).GetAll().FirstOrDefault(x => x.Id == game.DeveloperId);
            var str = $"Title: {game.Name}\nDescription: {game.Description}\nDeveloper: {developer.Name}\n" +
                      $"Category: {category.Title}Price: {game.Price}₴";
            await TgBotClient.Instance.Client.SendPhotoAsync(update.Message.From.Id,
                photo: game.PhotoLink ?? DEFAULT_GAME_ICON,
                caption: str,
                replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Order", "orderGame")));
        }
        else if (_gettingGamesByCategory)
        {
            _gettingGamesByCategory = false;
            int categoryId = new CategoriesController(_dbContext).GetAllCategories()
                .FirstOrDefault(x => x.Title == update.Message.Text).Id;
            if (categoryId == 0)
                await TgBotClient.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
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
                await TgBotClient.Instance.Client.SendPhotoAsync(update.Message.From.Id,
                    photo: game.PhotoLink ?? DEFAULT_GAME_ICON,
                    caption: str,
                    replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Order", "orderGame")));
            }
        }
        else if (_gettingGamesByDeveloper)
        {
            _gettingGamesByDeveloper = false;
            int developerId = new DeveloperController(_dbContext).GetAll()
                .FirstOrDefault(x => x.Name == update.Message.Text).Id;
            if (developerId == 0)
                await TgBotClient.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
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
                await TgBotClient.Instance.Client.SendPhotoAsync(update.Message.From.Id,
                    photo: game.PhotoLink ?? DEFAULT_GAME_ICON,
                    caption: str,
                    replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Order", "orderGame")));
            }
        }
        else if (_sendingMsgToClient)
        {
            if(_clientIdForSending == -1)
                return;
            _sendingMsgToClient = false;
            await TgBotClient.Instance.Client.SendTextMessageAsync(_clientIdForSending, update.Message.Text);
            _clientIdForSending = -1;
        }
        else if (_isSendingMessageToAll)
        {
            _isSendingMessageToAll = false;
            foreach (var user in new UsersController(_dbContext).GetAllUsers())
            {
                await TgBotClient.Instance.Client.SendTextMessageAsync(user.TgId, update.Message.Text);
            }
        }
    }
    
    private async Task DoCallBackQuery(Update update)
    {
        switch (update.CallbackQuery.Data)
        {
            case "addCategory":
                _isAddingCategory = true;
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.CallbackQuery.From.Id,
                    "Enter new category in format:\nTitle:content\nDescription:content\n");
                break;
            case "removeCategory":
                _isRemovingCategory = true;
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.CallbackQuery.From.Id,
                    "Enter category id for remove in format:\nId:content");
                break;
            case "addGame":
                _isAddingGame = true;
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.CallbackQuery.From.Id,
                    "Enter new game in format:\nName:content\nDescription:content\nPrice:content\nDeveloperId:content\n" +
                    "CategoryId:content\nPhotoLink:content\n");
                break;
            case "removeGame":
                _isRemovingGame = true;
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.CallbackQuery.From.Id,
                    "Enter game id for removing in format:\nId:content\n");
                break;
            case "addDeveloper":
                _isAddingDeveloper = true;
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.CallbackQuery.From.Id,
                    "Enter new developer in format:\nName:content\nDescription:content\nPhotoLink:content");
                break;
            case "removeDeveloper":
                _isRemovingDeveloper = true;
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.CallbackQuery.From.Id,
                    "Enter id developer for removing in format:\nId:content");
                break;
            case "showCategories":
                foreach (var category in new CategoriesController(_dbContext).GetAllCategories())
                {
                    var str = $"Id:{category.Id}\nTitle: {category.Title}\nDescription: {category.Description}";
                    await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.CallbackQuery.From.Id,
                        str,
                        replyMarkup: new InlineKeyboardMarkup(
                            InlineKeyboardButton.WithCallbackData("Remove", "removeCategory")));
                }

                break;
            case "showGames":
                var categories = new CategoriesController(_dbContext).GetAllCategories();
                var developers = new DeveloperController(_dbContext).GetAll();
                foreach (var game in new GamesController(_dbContext).GetAllGames())
                {
                    var category = categories.FirstOrDefault(x => x.Id == game.CategoryId);
                    var developer = developers.FirstOrDefault(x => x.Id == game.DeveloperId);
                    var str = $"Title: {game.Name}\nDescription: {game.Description}\nDeveloper: {developer.Name}" +
                              $"\nCategory: {category.Title}\nPrice: {game.Price}₴";
                    await TgBotAdmin.Instance.Client.SendPhotoAsync(update.CallbackQuery.From.Id,
                        photo: game.PhotoLink ?? DEFAULT_GAME_ICON,
                        caption: str,
                        replyMarkup: new InlineKeyboardMarkup(
                            InlineKeyboardButton.WithCallbackData("Remove", "removeGame")));
                }

                break;
            case "showDevelopers":
                foreach (var category in new DeveloperController(_dbContext).GetAll())
                {
                    var str = $"Id:{category.Id}\nTitle: {category.Name}\nDescription: {category.Description}";
                    await TgBotAdmin.Instance.Client.SendPhotoAsync(update.CallbackQuery.From.Id,
                        photo: category.PhotoLink ?? DEFAULT_DEVELOPER_ICON,
                        caption: str,
                        replyMarkup: new InlineKeyboardMarkup(
                            InlineKeyboardButton.WithCallbackData("Remove", "removeDeveloper")));
                }

                break;
            case "orderGame":
            {
                OrderGame(update);
                break;
            }
            case "sendMessageToClient":
            {
                _sendingMsgToClient = true;
                var strs = update.CallbackQuery.Message.Text.Split('\n');
                _clientIdForSending = Convert.ToInt64(strs.FirstOrDefault(x => x.Contains("Tg id:"))
                    .Substring(strs.FirstOrDefault(x => x.Contains("Tg id:")).IndexOf(':') + 1).Trim());
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.CallbackQuery.From.Id, "Enter a message");
                break;;
            }
            case "getActiveOrders":
            {
                foreach (var order in new OrdersController(_dbContext).GetInProcessOrders())
                {
                    string str = $"Id: {order.Id}\nGame Id: {order.GameId}\nUser id: {order.UserId}\nOrder date: {order.OrderDate}\n" +
                                 $"Orders status id: {order.OrderStatusId}";
                    await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.CallbackQuery.From.Id, str, 
                        replyMarkup:new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Move to history", "moveOrderToHistory")));
                }
                break;
            }
            case "getHistoryOrders":
            {
                foreach (var order in new OrdersController(_dbContext).GetCompleteOrders())
                {
                    string str = $"Id: {order.Id}\nGame Id: {order.GameId}\nUser id: {order.UserId}\nOrder date: {order.OrderDate}\n" +
                                 $"Orders status id: {order.OrderStatusId}";
                    await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.CallbackQuery.From.Id, str);
                }
                break;
            }
            case "moveOrderToHistory":
            {
                MoveOrderToHistory(update);
                break;
            }
        }
    }

    private async Task DoCommand(Update update)
    {
        switch (update.Message.Text)
        {
            case "/games":
            {
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
                    "Games:", replyMarkup: _gamesInlineMarkup);
                break;
            }
            case "/categories":
            {
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
                    "Game categories:", replyMarkup: _categoryInlineMarkup);
                break;
            }
            case "/developers":
            {
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
                    "Game developers:", replyMarkup: _developersInlineMarkup);
                break;
            }
            case "/orders":
            {
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
                    "Orders:", replyMarkup: _ordersInlineMarkup);
                break;
            }
            case "/getallclients":
            {
                foreach (var user in new UsersController(_dbContext).GetAllUsers())
                {
                    var str = $"Id: {user.Id}\nTg id: {user.TgId}\nUser name: {user.TgUserName}" +
                              $"\nFirst name: {user.FirstName}\nLast name: {user.LastName}";
                    await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.Message.From.Id,
                        str,
                        replyMarkup: new InlineKeyboardMarkup(
                            InlineKeyboardButton.WithCallbackData("Send message", "sendMessageToClient")));
                }
                break;
            }
            case "/getall":
            {
                var categories = new CategoriesController(_dbContext).GetAllCategories();
                var developers = new DeveloperController(_dbContext).GetAll();
                foreach (var game in new GamesController(_dbContext).GetAllGames())
                {
                    var category = categories.FirstOrDefault(x => x.Id == game.CategoryId);
                    var developer = developers.FirstOrDefault(x => x.Id == game.DeveloperId);
                    var str = $"Title: {game.Name}\nDescription: {game.Description}\nDeveloper: {developer.Name}" +
                              $"\nCategory: {category.Title}\nPrice: {game.Price}₴";
                    await TgBotClient.Instance.Client.SendPhotoAsync(update.Message.From.Id,
                        photo: game.PhotoLink ?? DEFAULT_GAME_ICON,
                        caption: str,
                        replyMarkup: new InlineKeyboardMarkup(
                            InlineKeyboardButton.WithCallbackData("Order", "orderGame")));
                }

                break;
            }
            case "/getallcategories":
            {
                foreach (var category in new CategoriesController(_dbContext).GetAllCategories())
                {
                    var str = $"Title: {category.Title}\nDescription: {category.Description}";
                    await TgBotClient.Instance.Client.SendTextMessageAsync(update.Message.From.Id, str);
                }

                break;
            }
            case "/getalldevelopers":
            {
                foreach (var category in new DeveloperController(_dbContext).GetAll())
                {
                    var str = $"Name: {category.Name}\nDescription: {category.Description}";
                    await TgBotClient.Instance.Client.SendTextMessageAsync(update.Message.From.Id, str);
                }

                break;
            }
            case "/getbyname":
            {
                await TgBotClient.Instance.Client.SendTextMessageAsync(update.Message.From.Id, "Enter game name:");
                _gettingGameByName = true;
                break;
            }
            case "/getbycategory":
            {
                await TgBotClient.Instance.Client.SendTextMessageAsync(update.Message.From.Id, "Enter category name:");
                _gettingGamesByCategory = true;
                break;
            }
            case "/getbydeveloper":
            {
                await TgBotClient.Instance.Client.SendTextMessageAsync(update.Message.From.Id, "Enter developer name:");
                _gettingGamesByDeveloper = true;
                break;
            }
            case "/start":
            {
                User user = new User()
                {
                    TgId = update.Message.From.Id,
                    TgUserName = update.Message.From.Username,
                    FirstName = update.Message.From.FirstName,
                    LastName = update.Message.From.LastName
                };
                new UsersController(_dbContext).AddUser(user);
                break;
            }
            case "/sendmessagetoall":
            {
                _isSendingMessageToAll = true;
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.Message.From.Id, "Enter a message");
                break;
            }
        }
    }

    private async void OrderGame(Update update)
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

    private async void MoveOrderToHistory(Update update)
    {
        var strings = update.CallbackQuery.Message.Text.Split('\n');
        var id = Convert.ToInt32(strings.FirstOrDefault(x => x.Contains("Id:")).
            Substring(strings.FirstOrDefault(x => x.Contains("Id:")).IndexOf(':')+1).Trim());
        new OrdersController(_dbContext).MoveOrderToComplete(id);
        await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.CallbackQuery.From.Id,
            "Order was moved to history");
    }
}