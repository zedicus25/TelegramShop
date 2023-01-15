using Telegram.Bot;

namespace TelegramShop.LocalControllers;

public class TgBotAdmin
{
    public static TgBotAdmin Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new TgBotAdmin();
                return _instance;
            }
            return _instance;
        }
    }

    private static TgBotAdmin _instance;

    public TelegramBotClient Client { get; private set; }

    private TgBotAdmin()
    {
        Client = new TelegramBotClient("5732449776:AAHj_xUT51foMIMMHm5QASrv719T_QSKzWs");
    }
}