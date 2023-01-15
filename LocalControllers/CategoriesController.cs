using TelegramShop.Models;

namespace TelegramShop.LocalControllers;

public class CategoriesController
{
    public void AddCategory(Category category)
    {
        using (GamesShopContext context = new GamesShopContext())
        {
            context.Categories.Add(category);
            context.SaveChanges();
        }
    }

    public void RemoveCategory(int id)
    {
        using (GamesShopContext context = new GamesShopContext())
        {
            var category = context.Categories.FirstOrDefault(x => x.Id == id);

            if (category != null)
            {
                var games = context.Games.Where(x => x.CategoryId == category.Id);
                context.Games.RemoveRange(games);
                context.Categories.Remove(category);
            }

            context.SaveChanges();
        }
    }

    public List<Category> GetAllCategories() => new GamesShopContext().Categories.ToList();

}