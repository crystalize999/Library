using Library.Data;
using Library.Models;
using Library.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Library.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LibraryContext _context;


        public CartController(LibraryContext context, CartService cartService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _cartService = cartService;
            _httpContextAccessor = httpContextAccessor;
        }

        
        public IActionResult Index()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartId = session.GetString("Id") ?? Guid.NewGuid().ToString();
            session.SetString("Id", cartId);
            var items = _cartService.GetAllCartItems(cartId);
            return View(items);
        }


        public IActionResult AddToCart(int id)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartId = session.GetString("Id") ?? Guid.NewGuid().ToString();
            session.SetString("Id", cartId);

            var selectedBook = _cartService.GetBookById(id);

            if (selectedBook != null)
            {
                _cartService.AddToCart(selectedBook, 1, cartId);
            }
            return RedirectToAction("Index", "Store");
        }

        public IActionResult RemoveFromCart(int id)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartId = session.GetString("Id") ?? Guid.NewGuid().ToString();
            var selectedBook = _cartService.GetBookById(id);
            if (selectedBook != null)
            {
                _cartService.RemoveFromCart(selectedBook, cartId);
            }
            return RedirectToAction("Index");
        }

        public IActionResult ReduceQuantity(int id)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartId = session.GetString("Id") ?? Guid.NewGuid().ToString();
            var selectedBook = _cartService.GetBookById(id);
            if (selectedBook != null)
            {
                var remainingQuantity = _cartService.ReduceQuantity(selectedBook, cartId);
                TempData["RemainingQuantity"] = remainingQuantity;
            }
            return RedirectToAction("Index");
        }

        public IActionResult IncreaseQuantity(int id)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartId = session.GetString("Id") ?? Guid.NewGuid().ToString();
            var selectedBook = _cartService.GetBookById(id);
            if (selectedBook != null)
            {
                var remainingQuantity = _cartService.IncreaseQuantity(selectedBook, cartId);
                TempData["RemainingQuantity"] = remainingQuantity;
            }
            return RedirectToAction("Index");
        }

        public IActionResult ClearCart()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartId = session.GetString("Id") ?? Guid.NewGuid().ToString();
            _cartService.ClearCart(cartId);
            return RedirectToAction("Index");
        }
        /* public void RemoveFromCart(Book book, string cartId)
         {
             var cartItem = _cartService.GetCartItem(book, cartId);

             if (cartItem != null)
             {
                 _context.CartItems.Remove(cartItem);
                 _context.SaveChanges();
             }
         }*/
    }
}
