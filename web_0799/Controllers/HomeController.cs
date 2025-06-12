using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using web_0799.Models;
using web_0799.Repositories;

namespace web_0799.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ProductDBContext _context;
        private readonly IProductRepository _productRepository;
        public HomeController(ILogger<HomeController> logger, ProductDBContext context, IProductRepository productRepository)
        {
            _logger = logger;
            _context = context;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                                         .Include(p => p.Category)
                                         .Include(p => p.Images)
                                         .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                                        .Include(p => p.Category)
                                        .Include(p => p.Images)
                                        .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        // GET: /Home/Search?query=abc
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                ViewBag.Query = query;
                return View(new List<Product>());
            }

            var products = await _context.Products
                                         .Include(p => p.Images)
                                         .Include(p => p.Category)
                                         .ToListAsync();

            var results = products
                .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            p.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            ViewBag.Query = query;
            return View(results);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
