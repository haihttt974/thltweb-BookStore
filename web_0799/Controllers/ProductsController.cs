using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web_0799.Models;
using web_0799.Repositories;

namespace web_0799.Controllers
{
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ProductDBContext _context;

        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, ProductDBContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    // Lưu hình ảnh đại diện tham khảo bài 02 hàm SaveImage

                    product.ImageUrl = await SaveImage(imageUrl);

                }
                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }
            // Nếu ModelState không hợp lệ, hiển thị form với dữ liệu đã nhập
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                var exists = await _context.Products.AnyAsync(p => p.Id == id);
                Console.WriteLine($"ID tồn tại trong bảng Products? {exists}");
            }
            return View(product);
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var uploadsFolder = Path.Combine("wwwroot/images");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return "/images/" + fileName;
        }


        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name",
            product.CategoryId);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Product product, IFormFile imageUrl, List<IFormFile> additionalImages, List<int> DeletedImageIds)
        {
            ModelState.Remove("ImageUrl");

            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);

                // Cập nhật các thuộc tính cơ bản
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;

                // Xử lý ảnh chính
                if (imageUrl != null)
                {
                    existingProduct.ImageUrl = await SaveImage(imageUrl);
                }

                // Xử lý ảnh phụ mới
                if (additionalImages != null && additionalImages.Any())
                {
                    existingProduct.Images ??= new List<ProductImage>();

                    foreach (var img in additionalImages)
                    {
                        var url = await SaveImage(img);
                        existingProduct.Images.Add(new ProductImage { Url = url, ProductId = existingProduct.Id });
                    }
                }

                // Xóa ảnh phụ nếu có yêu cầu
                if (DeletedImageIds != null && DeletedImageIds.Any())
                {
                    var imagesToRemove = existingProduct.Images
                        .Where(img => DeletedImageIds.Contains(img.Id))
                        .ToList();

                    foreach (var img in imagesToRemove)
                    {
                        // Optionally xóa file khỏi ổ đĩa
                        var path = Path.Combine("wwwroot", img.Url.TrimStart('/'));
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }

                        existingProduct.Images.Remove(img);
                    }
                }

                await _productRepository.UpdateAsync(existingProduct);
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        // Xử lý xóa sản phẩm
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.productImages.FindAsync(id);
            if (image == null)
                return NotFound();

            // Xóa file vật lý nếu cần
            var filePath = Path.Combine("wwwroot", image.Url.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.productImages.Remove(image);
            await _context.SaveChangesAsync();

            return Ok();
        }
        // GET: Products/Search
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return View(new List<Product>());
            }

            var products = await _productRepository.GetAllAsync();
            var results = products
                .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            p.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return View(results);
        }
    }
}
