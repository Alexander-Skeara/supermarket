namespace ObuvShop.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Today;
        public DateTime DeliveryDate { get; set; } = DateTime.Today.AddDays(3);
        public int PickupPointId { get; set; }
        public string PickupPointAddress { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string ClientFullName { get; set; } = string.Empty;
        public int PickupCode { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public string ArticlesSummary => Items.Count == 0
            ? "Состав не указан"
            : string.Join(", ", Items.Select(i => $"{i.ProductArticle} ({i.Quantity} шт.)"));

        public string ItemsDetailedSummary => Items.Count == 0
            ? "Нет позиций"
            : string.Join(Environment.NewLine, Items.Select(i => i.DisplayLine));
    }
}
