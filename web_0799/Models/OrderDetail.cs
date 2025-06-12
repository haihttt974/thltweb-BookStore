using web_0799.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_0799.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderId { get; set; }

        public Order Order { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [Range(1, 10000)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, 1000000000000.00)]
        public decimal UnitPrice { get; set; }
    }
}

