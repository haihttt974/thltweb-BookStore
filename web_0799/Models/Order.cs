using web_0799.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_0799.Models
{
    public class Order
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [Range(0.01, 1000000000000.00)]
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        public string Status { get; set; } // Ví dụ: "Pending", "Completed", "Cancelled"

        [StringLength(300)]
        public string? ShippingAddress { get; set; }
        
        public List<OrderDetail>? OrderDetails { get; set; }
    }
}

