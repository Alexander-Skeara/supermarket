using Microsoft.Data.SqlClient;
using ObuvShop.Models;

namespace ObuvShop.Data
{
    public class ProductRepository
    {
        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            const string sql = @"
                SELECT
                    p.Article,
                    p.ProductName,
                    p.UnitId,
                    u.UnitName,
                    p.Price,
                    p.SupplierId,
                    s.SupplierName,
                    p.ManufacturerId,
                    m.ManufacturerName,
                    p.CategoryId,
                    c.CategoryName,
                    p.Discount,
                    p.StockQuantity,
                    p.Description,
                    p.PhotoPath
                FROM dbo.Products p
                INNER JOIN dbo.Units u ON p.UnitId = u.UnitId
                INNER JOIN dbo.Suppliers s ON p.SupplierId = s.SupplierId
                INNER JOIN dbo.Manufacturers m ON p.ManufacturerId = m.ManufacturerId
                INNER JOIN dbo.Categories c ON p.CategoryId = c.CategoryId
                ORDER BY p.ProductName, p.Article";

            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                products.Add(new Product
                {
                    Article = reader.GetString(0),
                    ProductName = reader.GetString(1),
                    UnitId = reader.GetInt32(2),
                    UnitName = reader.GetString(3),
                    Price = reader.GetDecimal(4),
                    SupplierId = reader.GetInt32(5),
                    SupplierName = reader.GetString(6),
                    ManufacturerId = reader.GetInt32(7),
                    ManufacturerName = reader.GetString(8),
                    CategoryId = reader.GetInt32(9),
                    CategoryName = reader.GetString(10),
                    Discount = reader.GetByte(11),
                    StockQuantity = reader.GetInt32(12),
                    Description = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
                    PhotoPath = reader.IsDBNull(14) ? null : reader.GetString(14)
                });
            }

            return products;
        }

        public List<LookupItem> GetUnits() => GetLookup("SELECT UnitId, UnitName FROM dbo.Units ORDER BY UnitName");
        public List<LookupItem> GetSuppliers() => GetLookup("SELECT SupplierId, SupplierName FROM dbo.Suppliers ORDER BY SupplierName");
        public List<LookupItem> GetManufacturers() => GetLookup("SELECT ManufacturerId, ManufacturerName FROM dbo.Manufacturers ORDER BY ManufacturerName");
        public List<LookupItem> GetCategories() => GetLookup("SELECT CategoryId, CategoryName FROM dbo.Categories ORDER BY CategoryName");

        private List<LookupItem> GetLookup(string sql)
        {
            var list = new List<LookupItem>();
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new LookupItem { Id = reader.GetInt32(0), Name = reader.GetString(1) });
            }
            return list;
        }

        public bool ArticleExists(string article)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Products WHERE Article = @Article";
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Article", article.Trim());
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public bool IsProductInAnyOrder(string article)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.OrderItems WHERE ProductArticle = @Article";
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Article", article);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public void InsertProduct(Product p)
        {
            const string sql = @"
                INSERT INTO dbo.Products
                    (Article, ProductName, UnitId, Price, SupplierId, ManufacturerId, CategoryId, Discount, StockQuantity, Description, PhotoPath)
                VALUES
                    (@Article, @ProductName, @UnitId, @Price, @SupplierId, @ManufacturerId, @CategoryId, @Discount, @StockQuantity, @Description, @PhotoPath)";

            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            BindProductParameters(cmd, p);
            cmd.ExecuteNonQuery();
        }

        public void UpdateProduct(Product p)
        {
            const string sql = @"
                UPDATE dbo.Products
                SET ProductName = @ProductName,
                    UnitId = @UnitId,
                    Price = @Price,
                    SupplierId = @SupplierId,
                    ManufacturerId = @ManufacturerId,
                    CategoryId = @CategoryId,
                    Discount = @Discount,
                    StockQuantity = @StockQuantity,
                    Description = @Description,
                    PhotoPath = @PhotoPath
                WHERE Article = @Article";

            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            BindProductParameters(cmd, p);
            cmd.ExecuteNonQuery();
        }

        public void DeleteProduct(string article)
        {
            if (IsProductInAnyOrder(article))
            {
                throw new InvalidOperationException(
                    $"Товар с артикулом «{article}» присутствует в одном или нескольких заказах. Удаление запрещено.");
            }

            const string sql = "DELETE FROM dbo.Products WHERE Article = @Article";
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Article", article);
            cmd.ExecuteNonQuery();
        }

        private static void BindProductParameters(SqlCommand cmd, Product p)
        {
            cmd.Parameters.AddWithValue("@Article", p.Article.Trim());
            cmd.Parameters.AddWithValue("@ProductName", p.ProductName.Trim());
            cmd.Parameters.AddWithValue("@UnitId", p.UnitId);
            cmd.Parameters.AddWithValue("@Price", p.Price);
            cmd.Parameters.AddWithValue("@SupplierId", p.SupplierId);
            cmd.Parameters.AddWithValue("@ManufacturerId", p.ManufacturerId);
            cmd.Parameters.AddWithValue("@CategoryId", p.CategoryId);
            cmd.Parameters.AddWithValue("@Discount", (byte)p.Discount);
            cmd.Parameters.AddWithValue("@StockQuantity", p.StockQuantity);
            cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(p.Description) ? (object)DBNull.Value : p.Description.Trim());
            cmd.Parameters.AddWithValue("@PhotoPath", string.IsNullOrWhiteSpace(p.PhotoPath) ? (object)DBNull.Value : p.PhotoPath.Trim());
        }
    }
}
