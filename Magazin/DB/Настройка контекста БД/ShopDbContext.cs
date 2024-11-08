using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Reflection.Emit;

public class ShopDbContext : DbContext
{
    public ShopDbContext()
    {
        
    }
    public ShopDbContext(DbContextOptions<ShopDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderItem> OrderItems { get; set; }

    public DbSet<Payment> Payments { get; set; }

    public DbSet<Review> Reviews { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<PromoCode> PromoCodes { get; set; }

    public DbSet<UserAction> UserActions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // подключение к БД и логирование (для проверки подключения)
            optionsBuilder
             .UseSqlite("Data Source=C:\\Users\\User\\source\\repos\\Magazin\\Magazin\\shop.db")
             .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
             .EnableSensitiveDataLogging();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Дополнительная настройка моделей, если необходимо

        // Пример настройки уникального поля Code в PromoCode
        modelBuilder.Entity<PromoCode>()
            .HasIndex(p => p.Code)
            .IsUnique();

        // Включение каскадного удаления при удалении пользователя
        modelBuilder.Entity<User>()
            .HasMany(u => u.Orders)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
