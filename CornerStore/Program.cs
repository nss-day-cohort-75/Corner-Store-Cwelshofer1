using CornerStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using CornerStore.Models.DTO;
using Npgsql;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddNpgsql<CornerStoreDbContext>(builder.Configuration["CornerStoreDbConnectionString"] ?? "testing");

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.AllowTrailingCommas = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//endpoints go here

// Endpoint to create new cashiers in the database.
app.MapPost("/cashiers", (CornerStoreDbContext db, Cashier newCashier) =>
{
    try
    {
        db.Cashiers.Add(newCashier);
        db.SaveChanges();
        return Results.Created($"/cashiers/{newCashier.Id}", newCashier);
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest("Invalid data submitted");
    }
});

// Endpoint to get a cashier by their Id and includ their orders, orderProducts, and products.
app.MapGet("cashiers/{id}", (CornerStoreDbContext db, int id) =>
{
    try
    {
        return Results.Ok(db.Cashiers
            .Include(c => c.Orders)
            .ThenInclude(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .Select(c => new CashierDTO
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Orders = c.Orders.Select(o => new OrderDTO
                {
                    Id = o.Id,
                    CashierId = o.CashierId,
                    PaidOnDate = o.PaidOnDate,
                    OrderProducts = o.OrderProducts.Select(op => new OrderProductDTO
                    {
                        Id = op.Id,
                        ProductId = op.ProductId,
                        OrderId = op.OrderId,
                        Quantity = op.Quantity,
                        Product = new ProductDTO
                        {
                            Id = op.Product.Id,
                            ProductName = op.Product.ProductName,
                            Price = op.Product.Price,
                            Brand = op.Product.Brand,
                            CategoryId = op.Product.CategoryId
                        }
                    }).ToList()
                }).ToList()
            })
            .Single(c => c.Id == id));
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
});

// Endpoint to get products that returns only product names or category names that match the search string param.
app.MapGet("/products", (CornerStoreDbContext db, string search) =>
{
    var products = db.Products
        .Include(p => p.Category)
        .Where(p => p.ProductName.ToLower().Equals(search.ToLower()) ||
            (p.Category != null && p.Category.CategoryName != null && p.Category.CategoryName.ToLower().Equals(search.ToLower())))
        .Select(p => new ProductDTO
        {
            Id = p.Id,
            ProductName = p.ProductName,
            Price = p.Price,
            Brand = p.Brand,
            CategoryId = p.CategoryId,
            Category = new CategoryDTO
            {
                Id = p.Category.Id,
                CategoryName = p.Category.CategoryName
            }
        })
        .ToList();

    if (!products.Any())
    {
        return Results.BadRequest("No products found with exact match");
    }


    return Results.Ok(products);

});

// Endpoint to add a new product to the database.
app.MapPost("/products", (CornerStoreDbContext db, Product newProduct) =>
{
    try
    {
        db.Products.Add(newProduct);
        db.SaveChanges();
        return Results.Created($"/products/{newProduct.Id}", newProduct);
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest("Invalid data submitted");
    }
});

//Endpoint to get a update a product in the database.
app.MapPut("/api/products/{id}", (CornerStoreDbContext db, int id, Product product) =>
{
    try
    {
        Product productToUpdate = db.Products.SingleOrDefault(product => product.Id == id);
        if (productToUpdate == null)
        {
            return Results.NotFound();
        }
        productToUpdate.Id = product.Id;
        productToUpdate.ProductName = product.ProductName;
        productToUpdate.Price = product.Price;
        productToUpdate.Brand = product.Brand;
        productToUpdate.CategoryId = product.CategoryId;

        db.SaveChanges();
        return Results.NoContent();
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest("Invalid data submitted");
    }
});

// Endpoint to get an order including the cashier, order products, and products on the order with their category.
app.MapGet("/orders/{id}", (CornerStoreDbContext db, int id) =>
{
    try
    {
        return Results.Ok(db.Orders
            .Include(ca => ca.Cashier)
            .Include(op => op.OrderProducts)
            .ThenInclude(p => p.Product)
            .ThenInclude(c => c.Category)
            .Select(o => new OrderDTO
            {
                Id = o.Id,
                CashierId = o.CashierId,
                PaidOnDate = o.PaidOnDate,
                Cashier = new CashierDTO
                {
                    Id = o.Cashier.Id,
                    FirstName = o.Cashier.FirstName,
                    LastName = o.Cashier.LastName
                },
                OrderProducts = o.OrderProducts.Select(op => new OrderProductDTO
                {
                    Id = op.Id,
                    ProductId = op.ProductId,
                    OrderId = op.OrderId,
                    Quantity = op.Quantity,
                    Product = new ProductDTO
                    {
                        Id = op.Product.Id,
                        ProductName = op.Product.ProductName,
                        Price = op.Product.Price,
                        Brand = op.Product.Brand,
                        CategoryId = op.Product.CategoryId,
                        Category = new CategoryDTO
                        {
                            Id = op.Product.Category.Id,
                            CategoryName = op.Product.Category.CategoryName
                        }
                    }
                }).ToList()
            })
            .Single(o => o.Id == id));
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
});

//Endpoint to get orders that match a specific PaidOnDate if not it will return all orders.
app.MapGet("/orders", (CornerStoreDbContext db, string orderDate) =>
     {
         try
         {
             if (!string.IsNullOrEmpty(orderDate))
             {
                 if (DateTime.TryParse(orderDate, out DateTime requestedDate))
                 {
                     var filteredOrders = db.Orders
                         .Where(o => o.PaidOnDate.HasValue && o.PaidOnDate.Value.Date == requestedDate.Date)
                         .ToList();
                     return Results.Ok(filteredOrders);
                 }
                 else
                 {
                     return Results.BadRequest("Invalid 'orderDate' format");
                 }
             }
             else
             {
                 var allOrders = db.Orders.ToList();
                 return Results.Ok(allOrders);
             }
         }
         catch (Exception)
         {
             return Results.BadRequest("Invalid data submitted");
         }
     });

//Endpoint that deletes a single order from the database.
app.MapDelete("orders/{id}", (CornerStoreDbContext db, int id) =>
{
    Order o = db.Orders.FirstOrDefault(o => o.Id == id);
    if (o == null) return Results.NotFound();

    db.Orders.Remove(o);
    db.SaveChanges();
    return Results.Ok();
});

//Endpoint the posts a new order, along with  new orderproducts and new products that connect to it.
app.MapPost("/orders", (CornerStoreDbContext db, OrderDTO newOrderDTO) =>
{
    try
    {
        Order newOrder = new Order
        {
            CashierId = newOrderDTO.CashierId,
            PaidOnDate = newOrderDTO.PaidOnDate
        };

        db.Orders.Add(newOrder);
        db.SaveChanges();
        List<OrderProduct> orderProducts = new List<OrderProduct>();
        foreach (var orderProductDTO in newOrderDTO.OrderProducts)
        {
            OrderProduct newOrderProduct = new OrderProduct
            {
                OrderId = newOrder.Id,
                ProductId = orderProductDTO.ProductId,
                Quantity = orderProductDTO.Quantity
            };
            db.OrderProducts.Add(newOrderProduct);
            orderProducts.Add(newOrderProduct);
        }

        List<Product> products = new List<Product>();
        foreach (var productDTO in newOrderDTO.Products)
        {
            Product newProduct = new Product
            {
                ProductName = productDTO.ProductName,
                Price = productDTO.Price,
                Brand = productDTO.Brand,
                CategoryId = productDTO.CategoryId
            };
            db.Products.Add(newProduct);
            products.Add(newProduct);
        }


        db.SaveChanges();

        return Results.Created($"/orders/{newOrder.Id}", new
        {
            order = newOrder,
            products = products
        });
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest("Invalid data submitted");
    }
});





app.Run();

//don't move or change this!
public partial class Program { }
