using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace web_0799.Models
{
    public class ProductDBContext : IdentityDbContext<ApplicationUser>
    {
        public ProductDBContext(DbContextOptions<ProductDBContext> options) : base (options)
        {}
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> productImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<CartItemDb> CartItems { get; set; }
    }
}