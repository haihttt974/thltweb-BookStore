using web_0799.Models;

public class ShoppingCart
{
    public List<CartItem> Items { get; set; } = new List<CartItem>();

    public void AddItem(CartItem item)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            Items.Add(item);
        }
    }

    public void RemoveItem(int productId)
    {
        Items.RemoveAll(i => i.ProductId == productId);
    }

    // Bạn có thể thêm các phương thức khác như TínhTổngTiền(), CậpNhậtSốLượng(), XóaHếtGiỏHàng()...
}