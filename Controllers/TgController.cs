using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramShop.LocalControllers;

namespace TelegramShop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TgController
{
    [HttpPost]
    public async Task<IResult> Post([FromBody] Update update)
    {
        if (update != null)
        {
            if (update.Type == UpdateType.Message)
            {
                await TgBotClient.Instance.Client.SendTextMessageAsync(update.Message.From.Id, "Hello");
            }
        }

        return Results.Ok();
    }
}