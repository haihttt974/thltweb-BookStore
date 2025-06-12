using System.ComponentModel.DataAnnotations;

namespace web_0799.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }
        [Required]
        [Range(1, 10000)]
        public int Quantity { get; set; }
        [Required]
        [Range(0.01, 1000000000000.00)]
        public decimal UnitPrice { get; set; }
        [Required]
        public int ShoppingCartId { get; set; }
        public ShoppingCart ShoppingCart { get; set; }
    }
}