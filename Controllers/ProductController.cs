using Lab3.Models;
using Lab3.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Lab3.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IWebHostEnvironment hostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 4)
        {
            var totalItems = _productRepository.GetAll().Count();
            var products = _productRepository.GetAll()
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(products);
        }

        public IActionResult Details(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_categoryRepository.GetAll(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");

                    // Ensure the directory exists
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(fileStream);
                    }

                    product.ImageUrl = "/images/" + uniqueFileName;
                }

                _productRepository.Add(product);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_categoryRepository.GetAll(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        public IActionResult Edit(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_categoryRepository.GetAll(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = _productRepository.GetById(id);
                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    // Update product information
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.CategoryId = product.CategoryId;

                    // Handle image
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");

                        // Ensure the directory exists
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            imageFile.CopyTo(fileStream);
                        }

                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                        {
                            string oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, existingProduct.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        existingProduct.ImageUrl = "/images/" + uniqueFileName;
                    }

                    _productRepository.Update(existingProduct);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log error for debugging
                    Console.WriteLine($"Error updating product: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the product.");
                }
            }

            ViewBag.Categories = new SelectList(_categoryRepository.GetAll(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        public IActionResult Delete(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _productRepository.GetById(id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    string imagePath = Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _productRepository.Delete(id);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Search(string query, int page = 1, int pageSize = 4)
        {
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction("Index", new { page, pageSize });
            }

            var products = _productRepository.GetAll()
                .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            p.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)products.Count() / pageSize);
            ViewBag.Query = query;
            return View("Index", products);
        }
    }
}
