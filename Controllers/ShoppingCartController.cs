using Lab3.Models;
using Lab3.Repository;
using Microsoft.AspNetCore.Mvc;
using YourNameSpace.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
[Authorize]
public class ShoppingCartController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ShoppingCartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IProductRepository productRepository)
    {
        _productRepository = productRepository;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> AddToCart(int productId, int quantity)
    {
        var product = await GetProductFromDatabase(productId);
        if (product == null)
        {
            return NotFound();
        }

        var cartItem = new CartItem
        {
            Id = productId,
            Name = product.Name,
            Price = product.Price,
            Quantity = quantity
        };

        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
        cart.AddItem(cartItem);
        HttpContext.Session.SetObjectAsJson("Cart", cart);

        return RedirectToAction("Index");
    }

    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
        return View(cart);
    }

    public IActionResult RemoveFromCart(int productId)
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
        if (cart != null)
        {
            cart.RemoveItem(productId);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
        }
        return RedirectToAction("Index");
    }

    public IActionResult ClearCart()
    {
        HttpContext.Session.Remove("Cart");
        return RedirectToAction("Index");
    }

    public IActionResult Checkout()
    {
        return View(new Order());
    }
    [HttpPost]
    public IActionResult UpdateQuantity(int productId, int quantity)
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
        if (cart != null)
        {
            cart.UpdateItemQuantity(productId, quantity);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
        }
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(Order order)
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
        if (cart == null || !cart.Items.Any())
        {
            // Handle empty cart...
            return RedirectToAction("Index");
        }

        var user = await _userManager.GetUserAsync(User);
        order.UserId = user.Id;
        order.OrderDate = DateTime.UtcNow;
        order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
        order.OrderDetails = cart.Items.Select(i => new OrderDetail
        {
            ProductId = i.Id,
            Quantity = i.Quantity,
            Price = i.Price
        }).ToList();

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        HttpContext.Session.Remove("Cart");

        return View("OrderCompleted", order.Id); // Order completion confirmation page
    }

    private async Task<Product?> GetProductFromDatabase(int productId)
    {
        var product =  _productRepository.GetById(productId);
        return product;
    }
}
