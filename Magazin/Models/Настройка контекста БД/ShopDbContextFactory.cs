using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class ShopDbContextFactory : IDesignTimeDbContextFactory<ShopDbContext>
{
    public ShopDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ShopDbContext>();
        optionsBuilder.UseSqlite("Data Source=shop.db");

        return new ShopDbContext(optionsBuilder.Options);
    }
}
