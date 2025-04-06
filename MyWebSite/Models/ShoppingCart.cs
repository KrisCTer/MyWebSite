using MyWebSite.Models;

public class ShoppingCart
{
    public List<CartItem> Items { get; set; } = new List<CartItem>();
    public decimal TotalAmount => Items.Sum(item => item.Price * item.Quantity);
    public int TotalItems => Items.Sum(item => item.Quantity);

    public void AddItem(CartItem item)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity; // Tăng số lượng nếu sản phẩm đã tồn tại
        }
        else
        {
            Items.Add(item); // Thêm mới nếu sản phẩm chưa có
        }
    }

    public void RemoveItem(Guid productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
        }
    }
}
