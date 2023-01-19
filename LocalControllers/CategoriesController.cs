using TelegramShop.Data;
using TelegramShop.Models;

namespace TelegramShop.LocalControllers;

public class CategoriesController
{
    private GamesShopContext _dbContext;

    public CategoriesController(GamesShopContext context)
    {
        _dbContext = context;
    }

    public void AddCategory(Category category)
    {
        _dbContext.Categories.Add(category);
        _dbContext.SaveChanges();
    }

    public void RemoveCategory(int id)
    {
        var category = _dbContext.Categories.FirstOrDefault(x => x.Id == id);

        if (category != null)
        {
            var games = _dbContext.Games.Where(x => x.CategoryId == category.Id);
            _dbContext.Games.RemoveRange(games);
            _dbContext.Categories.Remove(category);
        }

        _dbContext.SaveChanges();
    }

    public List<Category> GetAllCategories() => _dbContext.Categories.ToList();
}