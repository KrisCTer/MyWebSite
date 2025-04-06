namespace MyWebSite.Models
{
    public class CartViewModel
    {
        public List<OrderDetail> CartItems { get; set; }
        public decimal CartTotal { get; set; }
        public string PromoCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalTotal => CartTotal - DiscountAmount;
    }
}
