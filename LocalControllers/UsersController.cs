using TelegramShop.Data;
using TelegramShop.Models;
using User = TelegramShop.Models.User;

namespace TelegramShop.LocalControllers;

public class UsersController
{
    private GamesShopContext _dbContext;

    public UsersController(GamesShopContext context)
    {
        _dbContext = context;
    }

    public void AddUser(User user)
    {
        if (_dbContext.Users.FirstOrDefault(x => x.TgId == user.TgId) != null)
            return;
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();
    }

    public User GetUserById(int id) => _dbContext.Users.FirstOrDefault(x => x.Id == id);

    public List<User> GetAllUsers() => _dbContext.Users.ToList();
}