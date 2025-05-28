using Microsoft.EntityFrameworkCore;
using CornerStore.Models;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
public class CornerStoreDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Cashier> Cashiers { get; set; }

    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context)
    {

    }

    //allows us to configure the schema when migrating as well as seed data
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {


        modelBuilder.Entity<Cashier>().HasData(new Cashier[]
        {
            new Cashier { Id = 1, FirstName = "Cash", LastName ="Checkman" },
            new Cashier { Id = 2, FirstName = "Mister", LastName = "Cashier" },
            new Cashier { Id = 3, FirstName = "Money", LastName = "Checkoutman" },
            new Cashier { Id = 4, FirstName = "Carl", LastName ="Cashback" },
            new Cashier { Id = 5, FirstName = "Luke", LastName ="Lottery" }
        });

        modelBuilder.Entity<Category>().HasData(new Category[]
       {
            new Category { Id = 1, CategoryName = "Energy Drinks"  },
            new Category { Id = 2, CategoryName = "Hot Food" },
            new Category { Id = 3, CategoryName = "Snacks"  },
            new Category { Id = 4, CategoryName = "Alcohol" },
            new Category { Id = 5, CategoryName = "Water" }
       });

        modelBuilder.Entity<Order>().HasData(new Order[]
        {
            new Order { Id = 1, CashierId = 1, PaidOnDate = new DateTime(2024, 8, 20, 19, 45, 0)  },
            new Order { Id = 2, CashierId = 2, PaidOnDate = new DateTime(2024, 7, 20, 19, 45, 0)  },
            new Order { Id = 3, CashierId = 3, PaidOnDate = new DateTime(2024, 7, 23, 19, 45, 0)  },
            new Order { Id = 4, CashierId = 4, PaidOnDate = new DateTime(2024, 7, 25, 19, 45, 0)  },
            new Order { Id = 5, CashierId = 5, PaidOnDate = new DateTime(2025, 7, 25, 19, 45, 0)  }
        });

        modelBuilder.Entity<OrderProduct>().HasData(new OrderProduct[]
       {
            new OrderProduct { Id = 1, ProductId = 1, OrderId = 1, Quantity = 20  },
            new OrderProduct { Id = 2, ProductId = 2, OrderId = 2, Quantity = 22  },
            new OrderProduct { Id = 3, ProductId = 3, OrderId = 3, Quantity = 33  },
            new OrderProduct { Id = 4, ProductId = 4, OrderId = 4, Quantity = 35  },
            new OrderProduct { Id = 5, ProductId = 5, OrderId = 5, Quantity = 30  }
       });


        modelBuilder.Entity<Product>().HasData(new Product[]
       {
            new Product { Id = 1, ProductName = "Supa Energy Drink", Price = 1.10M , Brand = "Supaa", CategoryId = 1 },
            new Product { Id = 2, ProductName = "Hotcakes", Price = 2, Brand = "Hotcakes Inc.", CategoryId = 2  },
            new Product { Id = 3, ProductName = "Star Crunch", Price = 3, Brand = "Little Debbie", CategoryId = 3  },
            new Product { Id = 4, ProductName = "Jonny Walker", Price = 4, Brand = "Jonny Walker Co.", CategoryId = 4 },
            new Product { Id = 5, ProductName = "Ultraa Water", Price = 5, Brand = "Ultraa", CategoryId = 5 }
       });


    }

 

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // This line tells EF Core to not automatically create indexes
        configurationBuilder.Conventions.Remove(typeof(ForeignKeyIndexConvention));
    }

}