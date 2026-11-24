using Microsoft.Data.SqlClient;
using ObuvShop.Models;

namespace ObuvShop.Data
{
    public class OrderRepository
    {
        public List<Order> GetAllOrders()
        {
            var orders = new List<Order>();
            var ordersMap = new Dictionary<int, Order>();

            const string sqlOrders = @"
                SELECT
                    o.OrderId,
                    o.OrderDate,
                    o.DeliveryDate,
                    o.PickupPointId,
                    pp.Address,
                    o.UserId,
                    u.FullName,
                    o.PickupCode,
                    o.StatusId,
                    os.StatusName
                FROM dbo.Orders o
                INNER JOIN dbo.PickupPoints pp ON o.PickupPointId = pp.PickupPointId
                INNER JOIN dbo.Users u ON o.UserId = u.UserId
                INNER JOIN dbo.OrderStatuses os ON o.StatusId = os.StatusId
                ORDER BY o.OrderId DESC";

            const string sqlItems = @"
                SELECT
                    oi.OrderItemId,
                    oi.OrderId,
                    oi.ProductArticle,
                    p.ProductName,
                    p.Price,
                    oi.Quantity
                FROM dbo.OrderItems oi
                INNER JOIN dbo.Products p ON oi.ProductArticle = p.Article
                ORDER BY oi.OrderItemId";

            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();

            using (var cmd = new SqlCommand(sqlOrders, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var order = new Order
                    {
                        OrderId = reader.GetInt32(0),
                        OrderDate = reader.GetDateTime(1),
                        DeliveryDate = reader.GetDateTime(2),
                        PickupPointId = reader.GetInt32(3),
                        PickupPointAddress = reader.GetString(4),
                        UserId = reader.GetInt32(5),
                        ClientFullName = reader.GetString(6),
                        PickupCode = reader.GetInt32(7),
                        StatusId = reader.GetInt32(8),
                        StatusName = reader.GetString(9),
                        Items = new List<OrderItem>()
                    };
                    orders.Add(order);
                    ordersMap[order.OrderId] = order;
                }
            }

            using (var cmd = new SqlCommand(sqlItems, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var orderId = reader.GetInt32(1);
                    if (ordersMap.TryGetValue(orderId, out var order))
                    {
                        order.Items.Add(new OrderItem
                        {
                            OrderItemId = reader.GetInt32(0),
                            OrderId = orderId,
                            ProductArticle = reader.GetString(2),
                            ProductName = reader.GetString(3),
                            UnitPrice = reader.GetDecimal(4),
                            Quantity = reader.GetInt32(5)
                        });
                    }
                }
            }

            return orders;
        }

        public List<LookupItem> GetPickupPoints()
        {
            var list = new List<LookupItem>();
            const string sql = "SELECT PickupPointId, Address FROM dbo.PickupPoints ORDER BY PickupPointId";
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new LookupItem { Id = reader.GetInt32(0), Name = reader.GetString(1) });
            return list;
        }

        public List<LookupItem> GetStatuses()
        {
            var list = new List<LookupItem>();
            const string sql = "SELECT StatusId, StatusName FROM dbo.OrderStatuses ORDER BY StatusId";
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new LookupItem { Id = reader.GetInt32(0), Name = reader.GetString(1) });
            return list;
        }

        public int GetNextPickupCode()
        {
            const string sql = "SELECT ISNULL(MAX(PickupCode), 900) + 1 FROM dbo.Orders";
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int InsertOrder(Order order)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                const string sqlOrder = @"
                    INSERT INTO dbo.Orders (OrderDate, DeliveryDate, PickupPointId, UserId, PickupCode, StatusId)
                    OUTPUT INSERTED.OrderId
                    VALUES (@OrderDate, @DeliveryDate, @PickupPointId, @UserId, @PickupCode, @StatusId)";

                int newOrderId;
                using (var cmd = new SqlCommand(sqlOrder, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@OrderDate", order.OrderDate.Date);
                    cmd.Parameters.AddWithValue("@DeliveryDate", order.DeliveryDate.Date);
                    cmd.Parameters.AddWithValue("@PickupPointId", order.PickupPointId);
                    cmd.Parameters.AddWithValue("@UserId", order.UserId);
                    cmd.Parameters.AddWithValue("@PickupCode", order.PickupCode);
                    cmd.Parameters.AddWithValue("@StatusId", order.StatusId);
                    newOrderId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                InsertOrderItems(conn, tx, newOrderId, order.Items);
                tx.Commit();
                return newOrderId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void UpdateOrder(Order order)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                const string sqlUpdate = @"
                    UPDATE dbo.Orders
                    SET OrderDate = @OrderDate,
                        DeliveryDate = @DeliveryDate,
                        PickupPointId = @PickupPointId,
                        UserId = @UserId,
                        PickupCode = @PickupCode,
                        StatusId = @StatusId
                    WHERE OrderId = @OrderId";

                using (var cmd = new SqlCommand(sqlUpdate, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@OrderId", order.OrderId);
                    cmd.Parameters.AddWithValue("@OrderDate", order.OrderDate.Date);
                    cmd.Parameters.AddWithValue("@DeliveryDate", order.DeliveryDate.Date);
                    cmd.Parameters.AddWithValue("@PickupPointId", order.PickupPointId);
                    cmd.Parameters.AddWithValue("@UserId", order.UserId);
                    cmd.Parameters.AddWithValue("@PickupCode", order.PickupCode);
                    cmd.Parameters.AddWithValue("@StatusId", order.StatusId);
                    cmd.ExecuteNonQuery();
                }

                using (var delCmd = new SqlCommand("DELETE FROM dbo.OrderItems WHERE OrderId = @OrderId", conn, tx))
                {
                    delCmd.Parameters.AddWithValue("@OrderId", order.OrderId);
                    delCmd.ExecuteNonQuery();
                }

                InsertOrderItems(conn, tx, order.OrderId, order.Items);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void DeleteOrder(int orderId)
        {
            const string sql = "DELETE FROM dbo.Orders WHERE OrderId = @OrderId";
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@OrderId", orderId);
            cmd.ExecuteNonQuery();
        }

        private static void InsertOrderItems(SqlConnection conn, SqlTransaction tx, int orderId, List<OrderItem> items)
        {
            const string sqlItem = @"
                INSERT INTO dbo.OrderItems (OrderId, ProductArticle, Quantity)
                VALUES (@OrderId, @ProductArticle, @Quantity)";

            foreach (var item in items)
            {
                using var cmd = new SqlCommand(sqlItem, conn, tx);
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                cmd.Parameters.AddWithValue("@ProductArticle", item.ProductArticle);
                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
