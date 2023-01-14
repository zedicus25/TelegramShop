using TelegramShop.Models;
using User = TelegramShop.Models.User;

namespace TelegramShop.LocalControllers;

public class UsersController
{
    public void AddUser(User user)
    {
        using (GamesShopContext context = new GamesShopContext())
        {
            context.Users.Add(user);
            context.SaveChanges();
        }
    }

    public User GetUserById(int id) => new GamesShopContext().Users.FirstOrDefault(x => x.Id == id);

    public List<User> GetAllUsers() => new GamesShopContext().Users.ToList();
}