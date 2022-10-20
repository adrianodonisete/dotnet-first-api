using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDbContext>(new ConfigDb().getStringCon());



var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);



app.MapGet("/products", () =>
{
    return Results.Ok(new { products = ProductRepository.Products });
});

app.MapPost("/products", (ProductRequest prodRequest, ApplicationDbContext context) =>
{
    var category = context.Categories.Where(c => c.Id == prodRequest.CategoryId).First();

    var product = new Product
    {
        Code = prodRequest.Code,
        Name = prodRequest.Name,
        Description = prodRequest.Description,
        Category = category
    };

    if (prodRequest.Tags != null)
    {
        product.Tags = new List<Tag>();
        foreach (var tag in prodRequest.Tags)
        {
            product.Tags.Add(new Tag { Name = tag });
        }
    }

    context.Products.Add(product);
    context.SaveChanges();
    return Results.Created($"/products/{product.Id}", new { product = product });
});

app.MapGet("/products/{id}", ([FromRoute] int id, ApplicationDbContext context) =>
{
    var productSaved = context.Products
        .Where(p => p.Id == id)
        .Include(p => p.Category)
        .Include(p => p.Tags);
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

app.MapGet("/", () => "Hello World 2!");

app.Run();
