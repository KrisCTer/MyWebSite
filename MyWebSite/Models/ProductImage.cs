namespace MyWebSite.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
