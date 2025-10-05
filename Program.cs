using KASHOP.Data;
using KASHOP.Models;
using KASHOP.Repositories; 
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Server=.;Database=KASHOP11;Trusted_Connection=True;TrustServerCertificate=True"));

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        context.Database.EnsureCreated();
        
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Electronics", Description = "Electronic devices and gadgets" },
                new Category { Name = "Clothing", Description = "Clothes and fashion accessories" },
                new Category { Name = "Books", Description = "Books and educational materials" },
                new Category { Name = "Home & Garden", Description = "Home appliances and tools" },
                new Category { Name = "Sports", Description = "Sports equipment and accessories" }
            );
            context.SaveChanges();
            Console.WriteLine("Categories added successfully!");
        }
        else
        {
            Console.WriteLine("Categories already exist in database.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing categories: {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();