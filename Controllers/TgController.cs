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
    private readonly TgControllerClient _tgControllerClient;
    private readonly TgControllerAdmin _tgControllerAdmin;
    
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
        _tgControllerClient = new TgControllerClient(_dbContext);
        _tgControllerAdmin = new TgControllerAdmin(_dbContext);
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
            _tgControllerAdmin.AddCategory(update.Message.Text, update.Message.From.Id);
        }
        else if (_isRemovingCategory)
        {
            _isRemovingCategory = false;
           _tgControllerAdmin.RemoveCategory(update.Message.Text, update.Message.From.Id);
        }
        else if (_isAddingDeveloper)
        {
            _isAddingDeveloper = false;
            _tgControllerAdmin.AddDeveloper(update.Message.Text, update.Message.From.Id);
        }
        else if (_isRemovingDeveloper)
        {
            _isRemovingDeveloper = false;
            _tgControllerAdmin.RemoveDeveloper(update.Message.Text, update.Message.From.Id);
        }
        else if (_isAddingGame)
        {
            _isAddingGame = false;
           _tgControllerAdmin.AddGame(update.Message.Text, update.Message.From.Id);
        }
        else if (_isRemovingGame)
        {
            _isRemovingGame = false;
           _tgControllerAdmin.RemoveGame(update.Message.Text, update.Message.From.Id);
        }
        else if (_gettingGameByName)
        {
            _gettingGameByName = false;
            _tgControllerClient.GetGameByName(update.Message.Text, update.Message.From.Id);
        }
        else if (_gettingGamesByCategory)
        {
            _gettingGamesByCategory = false;
            _tgControllerClient.GetGamesByCategory(update.Message.Text, update.Message.From.Id);
        }
        else if (_gettingGamesByDeveloper)
        {
            _gettingGamesByDeveloper = false;
            _tgControllerClient.GetGamesDeveloper(update.Message.Text, update.Message.From.Id);
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
                _tgControllerAdmin.GetAllCategories(update.CallbackQuery.From.Id);
                break;
            case "showGames":
                _tgControllerAdmin.GetAllGames(update.CallbackQuery.From.Id);
                break;
            case "showDevelopers":
                _tgControllerAdmin.GetAllDevelopers(update.CallbackQuery.From.Id);
                break;
            case "orderGame":
            {
                _tgControllerClient.OrderGame(update);
                break;
            }
            case "sendMessageToClient":
            {
                _sendingMsgToClient = true;
                var strs = update.CallbackQuery.Message.Text.Split('\n');
                _clientIdForSending = Convert.ToInt64(strs.FirstOrDefault(x => x.Contains("Tg id:"))
                    .Substring(strs.FirstOrDefault(x => x.Contains("Tg id:")).IndexOf(':') + 1).Trim());
                await TgBotAdmin.Instance.Client.SendTextMessageAsync(update.CallbackQuery.From.Id, "Enter a message");
                break;
            }
            case "getActiveOrders":
            {
                _tgControllerAdmin.GetActiveOrders(update.CallbackQuery.From.Id);
                break;
            }
            case "getHistoryOrders":
            {
               _tgControllerAdmin.GetHistoryData(update.CallbackQuery.From.Id);
                break;
            }
            case "moveOrderToHistory":
            {
                _tgControllerAdmin.MoveOrderToHistory(update);
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
               _tgControllerAdmin.GetAllClients(update.Message.From.Id);
                break;
            }
            case "/getall":
            {
                _tgControllerClient.GetAllGames(update.Message.From.Id);
                break;
            }
            case "/getallcategories":
            {
                _tgControllerClient.GetAllCategories(update.Message.From.Id);
                break;
            }
            case "/getalldevelopers":
            {
                _tgControllerClient.GetAllDevelopers(update.Message.From.Id);
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
}