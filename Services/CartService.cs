using Library.Data;
using Library.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.Services
{
    public class CartService
    {
        private readonly LibraryContext _context;

        public CartService(LibraryContext context)
        {
            _context = context;
        }

        public Cart GetCart(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Cart ID cannot be null or empty.", nameof(id));
            }

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefault(c => c.Id == id);

            if (cart == null)
            {
                cart = new Cart { Id = id };

                if (string.IsNullOrEmpty(cart.Id))
                {
                    throw new InvalidOperationException("Cart ID cannot be null or empty after creation.");
                }

                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            return cart;
        }

        public List<CartItem> GetAllCartItems(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Cart ID cannot be null or empty.", nameof(id));
            }

            var cart = GetCart(id);
            return cart.CartItems;
        }

        public int GetCartTotal(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Cart ID cannot be null or empty.", nameof(id));
            }

            var cartItems = GetAllCartItems(id);

            return cartItems
                .Select(ci => ci.Book.Price * ci.Quantity)
                .Sum();
        }

        public Book GetBookById(int id)
        {
            return _context.Books.FirstOrDefault(b => b.Id == id);
        }

        public CartItem GetCartItem(Book book, string cartId)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            if (string.IsNullOrEmpty(cartId))
            {
                throw new ArgumentException("Cart ID cannot be null or empty.", nameof(cartId));
            }

            return _context.CartItems.SingleOrDefault(ci => ci.Book.Id == book.Id && ci.CartId == cartId);
        }

        public void AddToCart(Book book, int quantity, string cartId)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            if (string.IsNullOrEmpty(cartId))
            {
                throw new ArgumentException("Cart ID cannot be null or empty.", nameof(cartId));
            }

            var cart = GetCart(cartId); // Ensure the cart exists or create it
            var cartItem = GetCartItem(book, cartId);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    Book = book,
                    Quantity = quantity,
                    CartId = cartId
                };

                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            _context.SaveChanges();
        }

        public int ReduceQuantity(Book book, string cartId)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            if (string.IsNullOrEmpty(cartId))
            {
                throw new ArgumentException("Cart ID cannot be null or empty.", nameof(cartId));
            }

            var cartItem = GetCartItem(book, cartId);
            var remainingQuantity = 0;

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    remainingQuantity = --cartItem.Quantity;
                }
                else
                {
                    _context.CartItems.Remove(cartItem);
                }

                _context.SaveChanges();
            }

            return remainingQuantity;
        }

        public int IncreaseQuantity(Book book, string cartId)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            if (string.IsNullOrEmpty(cartId))
            {
                throw new ArgumentException("Cart ID cannot be null or empty.", nameof(cartId));
            }

            var cartItem = GetCartItem(book, cartId);
            var remainingQuantity = 0;

            if (cartItem != null)
            {
                remainingQuantity = ++cartItem.Quantity;
                _context.SaveChanges();
            }

            return remainingQuantity;
        }

        public void RemoveFromCart(Book book, string cartId)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            if (string.IsNullOrEmpty(cartId))
            {
                throw new ArgumentException("Cart ID cannot be null or empty.", nameof(cartId));
            }

            var cartItem = GetCartItem(book, cartId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();
            }
        }

        public void ClearCart(string cartId)
        {
            if (string.IsNullOrEmpty(cartId))
            {
                throw new ArgumentException("Cart ID cannot be null or empty.", nameof(cartId));
            }

            var cartItems = _context.CartItems.Where(ci => ci.CartId == cartId).ToList();
            _context.CartItems.RemoveRange(cartItems);
            _context.SaveChanges();
        }
    }
}
