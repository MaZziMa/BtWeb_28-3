using Lab3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab3.Controllers
{
    public class ProductImageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductImageController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index(int productId)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.ProductId = productId;
            ViewBag.ProductName = product.Name;

            return View(product.ProductImages);
        }

        public IActionResult Create(int productId)
        {
            ViewBag.ProductId = productId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("", "Please select an image file.");
                ViewBag.ProductId = productId;
                return View();
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

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
                await imageFile.CopyToAsync(fileStream);
            }

            var productImage = new ProductImage
            {
                ImageUrl = "/images/" + uniqueFileName,
                ProductId = productId
            };

            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { productId });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var productImage = await _context.ProductImages.FindAsync(id);
            if (productImage == null)
            {
                return NotFound();
            }

            return View(productImage);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productImage = await _context.ProductImages.FindAsync(id);
            if (productImage == null)
            {
                return NotFound();
            }

            int productId = productImage.ProductId;

            // Delete the image file
            if (!string.IsNullOrEmpty(productImage.ImageUrl))
            {
                string imagePath = Path.Combine(_hostEnvironment.WebRootPath, productImage.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.ProductImages.Remove(productImage);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { productId });
        }
    }
}

