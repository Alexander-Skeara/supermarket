namespace ObuvShop.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public string ProductArticle { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public string DisplayLine => $"• {ProductArticle} — {ProductName}: {Quantity} шт.";
    }
}
