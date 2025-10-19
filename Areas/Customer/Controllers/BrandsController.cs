using Microsoft.AspNetCore.Mvc;
using KASHOP.Data;
using KASHOP.Models;
using Microsoft.EntityFrameworkCore;

namespace KASHOP.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class BrandsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BrandsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customer/Brands
        public async Task<IActionResult> Index()
        {
            var brands = await _context.Brands
                .Where(b => b.IsActive)
                .Include(b => b.Products)
                .ToListAsync();
            
            return View(brands);
        }

        // GET: Customer/Brands/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var brand = await _context.Brands
                .Include(b => b.Products)
                    .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);

            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // GET: Customer/Brands/Products/5
        public async Task<IActionResult> Products(int id)
        {
            var brand = await _context.Brands
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);

            if (brand == null)
            {
                return NotFound();
            }

            var products = await _context.Products
                .Where(p => p.BrandId == id && p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .ToListAsync();

            ViewBag.BrandName = brand.Name;
            return View(products);
        }
    }
}