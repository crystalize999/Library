using Library.Data;
using Library.Services;
using Microsoft.EntityFrameworkCore;

namespace Library.Models
{
    public class Cart
    {

        public string Id {  get; set; }
        /*public List<CartItem> CartItems { get; set; }*/
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
