namespace MyWebSite.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal Price { get; set; }
        public decimal Discount { get; set; } = 0;
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
