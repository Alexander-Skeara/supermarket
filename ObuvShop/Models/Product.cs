namespace ObuvShop.Models
{
    public class Product
    {
        public string Article { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int Discount { get; set; }
        public int StockQuantity { get; set; }
        public string Description { get; set; } = string.Empty;
        public string PhotoPath { get; set; }

        public bool HasDiscount => Discount > 0;
        public bool IsHighDiscount => Discount > 15;
        public bool IsOutOfStock => StockQuantity == 0;
        public decimal FinalPrice => Math.Round(Price * (100 - Discount) / 100m, 2);
        public string TitleDisplay => $"{CategoryName} | {ProductName}";
        public string StockDisplay => StockQuantity > 0
            ? $"{StockQuantity} {UnitName}"
            : "Нет в наличии (0)";
        public string SelectorDisplay => $"{Article} — {ProductName} ({ManufacturerName}, {FinalPrice:N2} ₽)";
    }
}
