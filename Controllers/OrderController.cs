using Library.Data;
using Library.Models;
using Library.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Library.Controllers
{
    public class OrderController : Controller
    {
        private readonly LibraryContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CartService _cartService;

        public OrderController(LibraryContext context, CartService cartService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _cartService = cartService;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Checkout()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartId = session.GetString("Id");
            if (string.IsNullOrEmpty(cartId))
            {
                // Обработка случая, когда корзина пуста
                return RedirectToAction("Index", "Cart");
            }

            var cartItems = _cartService.GetAllCartItems(cartId);
            var order = new Order
            {
                // Установка каких-то значений для модели Order
            };

            return View(order);
        }


        [HttpPost]
        public IActionResult CheckoutComplete()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartId = session.GetString("Id");
            if (string.IsNullOrEmpty(cartId))
            {
                // Обработка случая, когда корзина пуста
                return RedirectToAction("Index", "Cart");
            }

            var cartItems = _cartService.GetAllCartItems(cartId);
            // Создание заказа на основе товаров в корзине
            var order = new Order
            {
                UserId = "TODO", // Здесь нужно получить идентификатор пользователя
                OrderDate = DateTime.Now,
                OrderItems = cartItems.Select(ci => new OrderItem
                {
                    BookId = ci.Book.Id,
                    Quantity = ci.Quantity,
                }).ToList()
            };

            order.OrderTotal = order.OrderItems.Sum(oi => oi.Price * oi.Quantity);
            // Добавление заказа в базу данных
            _context.Orders.Add(order);
            _context.SaveChanges();

            // Очистка корзины
            _cartService.ClearCart(cartId);

            return View(order);
        }

        [HttpPost]
        public IActionResult CreateOrder(Order order)
        {
            order.OrderDate = DateTime.Now;

            var cartItems = _context.CartItems;

            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem()
                {
                    Quantity = item.Quantity,
                    BookId = item.Book.Id,
                    OrderId = order.Id,
                    Price = item.Book.Price * item.Quantity
                };
                order.OrderItems.Add(orderItem);
                order.OrderTotal += orderItem.Price;
            }
            _context.Orders.Add(order);
            _context.SaveChanges();
            // Логика создания заказа из админ-панели или административной зоны
            return View();
        }
    }
}
