using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebSite.Models
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, StringLength(100)]
        public string Name { get; set; }
        [Range(0.01, 10000.00)]
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int StockQuantity { get; set; }
        public int LowStockThreshold { get; set; } = 5;
        public string? ImageUrl { get; set; }
        public HashSet<ProductImage>? Images { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        public List<Review>? Reviews { get; set; }
        public ProductDetail? ProductDetail { get; set; }
    }
}
