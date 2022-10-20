using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Data.SqlClient;

using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["Database:SqlServer"]);
builder.Services.AddSqlServer<ApplicationDbContext>(new ConfigDb().getStringCon());



var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);

app.MapGet("/", () => "Hello World 2!");

app.MapGet("/products", () =>
{
    return Results.Ok(new { products = ProductRepository.Products });
});

app.MapPost("/products", (Product product) =>
{
    ProductRepository.Add(product);
    return Results.Created($"/products/{product.Code}", new { product = product });
});

// app.com/getproduct/10a
app.MapGet("/products/{code}", ([FromRoute] string code) =>
{
    var productSaved = ProductRepository.ByCode(code);
    if (productSaved == null)
    {
        // Console.WriteLine("not found");
        return Results.NotFound(new { message = "Not Found" });
    }
    return Results.Ok(new { product = productSaved });
});

app.MapPut("/products/{code}", ([FromRoute] string code, Product product) =>
{
    var productSaved = ProductRepository.ByCode(code);
    if (productSaved == null)
    {
        return Results.NotFound(new { message = "Not Found" });
    }
    productSaved.Name = product.Name;

    return Results.Ok(new { product = productSaved });
});

app.MapDelete("/products/{code}", ([FromRoute] string code) =>
{
    var productSaved = ProductRepository.ByCode(code);
    if (productSaved == null)
    {
        return Results.NotFound(new { message = "Not Found" });
    }

    ProductRepository.Remove(productSaved);
    return Results.Ok(new { message = "Deleted" });
});

// app.com/getproduct/dateIni=1&dateEnd=10
app.MapGet("/getproduct", ([FromQuery] string dateIni, [FromQuery] string dateEnd) =>
{
    return dateIni + " - " + dateEnd;
});

app.MapGet("/getheader", (HttpRequest request) =>
{
    return request.Headers["product-code"].ToString() + " outro";
});

app.MapPost("/user", () => new
{
    name = "Adriano machado",
    Age = 45
});

app.MapGet("/AddHeader", (HttpResponse response) =>
{
    response.Headers.Add("teste", "adriano");
    return new { name = "Adriano com header" };
});

if (app.Environment.IsStaging())
{
    app.MapGet("/config/database", (IConfiguration config) =>
    {
        return Results.Ok(new { info = config["Database:Connection"] });
    });
}

app.Run();


public static class ProductRepository
{
    public static List<Product> Products { get; set; } = new List<Product>();

    public static void Init(IConfiguration config)
    {
        var products = config.GetSection("Products").Get<List<Product>>();
        Products = products;
    }

    public static void Add(Product product)
    {
        Products.Add(product);
    }

    public static void Remove(Product product)
    {
        Products.Remove(product);
    }

    public static Product ByCode(string code)
    {
        return Products.FirstOrDefault(p => p.Code == code);
    }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Info { get; set; }
    public int ProductId { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    public List<Tag> Tags { get; set; }
}


public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Product>()
            .Property(p => p.Code).HasMaxLength(20).IsRequired();
        builder.Entity<Product>()
            .Property(p => p.Name).HasMaxLength(120).IsRequired();
        builder.Entity<Product>()
            .Property(p => p.Description).HasMaxLength(500).IsRequired(false);

    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // apagar
    // protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlServer(
    //     new ConfigDb().getStringCon()
    //     );
}

public class ConfigDb
{
    public string getStringCon()
    {
        // return "Server=localhost;Database=Products;User id=sa ;Password: @Sql2022 ;Encrypt=YES;TrustServerCertificate=YES;MultipleActiveResultSets=true";
        var builderConn = new SqlConnectionStringBuilder
        {
            DataSource = "localhost",
            InitialCatalog = "Products",
            UserID = "sa",
            Password = "@Sql2022"
        };

        return builderConn.ConnectionString;

        // var builder = WebApplication.CreateBuilder();
        // var builderConn = new SqlConnectionStringBuilder
        // {
        //     DataSource = builder.Configuration["Database:SqlServer:DataSource"],
        //     InitialCatalog = builder.Configuration["Database:SqlServer:InitialCatalog"],
        //     UserID = builder.Configuration["Database:SqlServer:UserID"],
        //     Password = builder.Configuration["Database:SqlServer:Password"]
        // };

        // return builderConn.ConnectionString;
    }
}