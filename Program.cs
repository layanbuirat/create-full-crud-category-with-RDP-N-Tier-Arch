using KASHOP.Data;
using KASHOP.Models;
using KASHOP.Repositories;
using KASHOP.Services;
using KASHOP.Services.Interfaces;
using KASHOP.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=.;Database=KASHOP11;Trusted_Connection=True;TrustServerCertificate=True";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Identity Services
builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add Repository Services
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();

// Add Business Services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IBrandService, BrandService>();

// Add HttpContextAccessor for file uploads
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllersWithViews();

// Add Razor Pages for Identity
builder.Services.AddRazorPages();

var app = builder.Build();

// 🔥 تهيئة قاعدة البيانات والبيانات الأولية
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // تأكد من إنشاء قاعدة البيانات وتطبيق المigrations
        await context.Database.MigrateAsync();
        
        // إضافة البيانات الأولية
        await SeedData(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Error initializing database");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Configure Areas Routing
app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Home}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "Customer", 
    areaName: "Customer",
    pattern: "Customer/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "MyArea",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages for Identity
app.MapRazorPages();

app.Run();

// وظائف التهيئة
async Task SeedData(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    // إضافة الأدوار إذا لم تكن موجودة
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    
    if (!await roleManager.RoleExistsAsync("Customer"))
    {
        await roleManager.CreateAsync(new IdentityRole("Customer"));
    }
    Console.WriteLine("✅ Roles added successfully!");

    // إضافة مستخدم Admin إذا لم يكن موجوداً
    if (await userManager.FindByEmailAsync("admin@kashop.com") == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = "admin@kashop.com",
            Email = "admin@kashop.com",
            FirstName = "Admin",
            LastName = "User",
            PhoneNumber = "1234567890",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine("✅ Admin user created successfully!");
        }
        else
        {
            Console.WriteLine($"❌ Error creating admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    // إضافة الفئات إذا لم تكن موجودة
    if (!context.Categories.Any())
    {
        context.Categories.AddRange(
            new Category { 
                Name = "Electronics", 
                Description = "Electronic devices and gadgets",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Category { 
                Name = "Clothing", 
                Description = "Clothes and fashion accessories",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Category { 
                Name = "Books", 
                Description = "Books and educational materials",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Category { 
                Name = "Home & Garden", 
                Description = "Home appliances and tools",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Category { 
                Name = "Sports", 
                Description = "Sports equipment and accessories",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();
        Console.WriteLine("✅ Categories added successfully!");
    }

    // إضافة العلامات التجارية إذا لم تكن موجودة
    if (!context.Brands.Any())
    {
        context.Brands.AddRange(
            new Brand { 
                Name = "Apple", 
                Description = "Technology company", 
                LogoUrl = "/images/brands/apple.png",
                Website = "https://www.apple.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Brand { 
                Name = "Nike", 
                Description = "Sports apparel and equipment", 
                LogoUrl = "/images/brands/nike.png",
                Website = "https://www.nike.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Brand { 
                Name = "Samsung", 
                Description = "Electronics and home appliances", 
                LogoUrl = "/images/brands/samsung.png",
                Website = "https://www.samsung.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Brand { 
                Name = "Adidas", 
                Description = "Sports shoes and clothing", 
                LogoUrl = "/images/brands/adidas.png",
                Website = "https://www.adidas.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Brand { 
                Name = "HP", 
                Description = "Computers and printers", 
                LogoUrl = "/images/brands/hp.png",
                Website = "https://www.hp.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();
        Console.WriteLine("✅ Brands added successfully!");
    }

    // إضافة منتجات نموذجية إذا لم تكن موجودة
    if (!context.Products.Any())
    {
        var electronicsCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Electronics");
        var appleBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Apple");
        var samsungBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Samsung");
        var hpBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "HP");

        if (electronicsCategory != null)
        {
            context.Products.AddRange(
                new Product
                {
                    Name = "HP ZBook",
                    Description = "High-performance workstation laptop",
                    Price = 4500.00m,
                    Stock = 8,
                    CategoryId = electronicsCategory.Id,
                    BrandId = hpBrand?.Id,
                    SKU = "HP-ZBOOK-001",
                    IsFeatured = false,
                    IsActive = true,
                    ImageUrl = "/images/products/hp-zbook.jpg",
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Coffee Machine",
                    Description = "Has storage need one Casula each day Weight: 2kg, Color: Black",
                    Price = 998.00m,
                    Stock = 8,
                    CategoryId = electronicsCategory.Id,
                    BrandId = samsungBrand?.Id,
                    SKU = "COFFEE-MACH-001",
                    IsFeatured = false,
                    IsActive = true,
                    ImageUrl = "/images/products/coffee-machine.jpg",
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "iPhone 15 Pro",
                    Description = "Latest Apple smartphone with advanced features",
                    Price = 1200.00m,
                    Stock = 15,
                    CategoryId = electronicsCategory.Id,
                    BrandId = appleBrand?.Id,
                    SKU = "IPHONE-15-PRO",
                    IsFeatured = true,
                    IsActive = true,
                    ImageUrl = "/images/products/iphone15.jpg",
                    CreatedAt = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Sample products added successfully!");
        }
    }

    Console.WriteLine("🎉 Database seeding completed successfully!");
}