using Telegram.Bot;

namespace TelegramShop.LocalControllers
{
    public class TgBotClient
    {
        public static TgBotClient Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new TgBotClient();
                    return _instance;
                }
                return _instance;
            }
        }

        private static TgBotClient _instance;

        public TelegramBotClient Client { get; private set; }

        private TgBotClient()
        {
            Client = new TelegramBotClient("5915923107:AAFs6Wq2KIOT87YZ70LkaX4-0SS8A_er8XU");
        }
    }
}
