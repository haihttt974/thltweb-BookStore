using System.ComponentModel.DataAnnotations;

namespace web_0799.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public List<CartItem> CartItems { get; set; }
    }
}