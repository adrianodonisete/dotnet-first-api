using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDbContext>(new ConfigDb().getStringCon());



var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);


// all
app.MapGet("/products", (ApplicationDbContext context) =>
{
    return Results.Ok(new { products = context.Products });
});

// create
app.MapPost("/products", (ProductRequest prodRequest, ApplicationDbContext context) =>
{
    var category = context.Categories.Where(c => c.Id == prodRequest.CategoryId).FirstOrDefault();

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

// get one
app.MapGet("/products/{id}", ([FromRoute] int id, ApplicationDbContext context) =>
{
    var productSaved = context.Products
        .Where(p => p.Id == id)
        .Include(p => p.Category)
        .Include(p => p.Tags)
        .FirstOrDefault();

    if (productSaved == null)
    {
        return Results.NotFound(new { message = "Not Found" });
    }

    return Results.Ok(new { product = productSaved });
});

// update
app.MapPut("/products/{id}", ([FromRoute] int id, ProductRequest prodRequest, ApplicationDbContext context) =>
{
    var productSaved = context.Products
        .Where(p => p.Id == id)
        .Include(p => p.Category)
        .Include(p => p.Tags)
        .FirstOrDefault();
    if (productSaved == null)
    {
        return Results.NotFound(new { message = "Not Found" });
    }

    if (prodRequest.Code != null)
    {
        productSaved.Code = prodRequest.Code;
    }
    if (prodRequest.Name != null)
    {
        productSaved.Name = prodRequest.Name;
    }
    if (prodRequest.Description != null)
    {
        productSaved.Description = prodRequest.Description;
    }
    var category = context.Categories.Where(c => c.Id == prodRequest.CategoryId).FirstOrDefault();
    if (category != null)
    {
        productSaved.Category = category;
    }
    if (prodRequest.Tags != null)
    {
        productSaved.Tags = new List<Tag>();
        foreach (var tag in prodRequest.Tags)
        {
            productSaved.Tags.Add(new Tag { Name = tag });
        }
    }
    context.SaveChanges();

    return Results.Ok(new { product = productSaved });
});

// delete
app.MapDelete("/products/{id}", ([FromRoute] int id, ApplicationDbContext context) =>
{
    var productSaved = context.Products
        .Where(p => p.Id == id)
        .FirstOrDefault();
    if (productSaved == null)
    {
        return Results.NotFound(new { message = "Not Found" });
    }

    context.Products.Remove(productSaved);
    context.SaveChanges();
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
