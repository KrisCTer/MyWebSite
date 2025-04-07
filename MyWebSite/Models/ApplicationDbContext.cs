using System.Diagnostics;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MyWebSite.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<DiscountCode> discountCodes { get; set; }
        public DbSet<UserDiscountCode> UserDiscountCodes { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<LoyalCustomer> LoyalCustomers { get; set; }
        public DbSet<Activities> Activities { get; set; }
        public DbSet<Payment> Payments { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình mối quan hệ many-to-many giữa User và DiscountCode
            builder.Entity<UserDiscountCode>()
                .HasKey(udc => new { udc.UserId, udc.DiscountCodeId });  // Định nghĩa khóa chính composite

            builder.Entity<UserDiscountCode>()
                .HasOne(udc => udc.User)
                .WithMany(u => u.UserDiscountCodes)
                .HasForeignKey(udc => udc.UserId);

            builder.Entity<UserDiscountCode>()
                .HasOne(udc => udc.DiscountCode)
                .WithMany(d => d.UserDiscountCodes)
                .HasForeignKey(udc => udc.DiscountCodeId);
        }
    }
}
