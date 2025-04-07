namespace MyWebSite.Models
{
    public class Cart
    {
        public string Id { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
    }
}
