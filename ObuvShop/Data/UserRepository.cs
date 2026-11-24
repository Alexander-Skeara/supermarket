using Microsoft.Data.SqlClient;
using ObuvShop.Models;

namespace ObuvShop.Data
{
    public class UserRepository
    {
        public User Authenticate(string login, string password)
        {
            const string sql = @"
                SELECT u.UserId, u.RoleId, r.RoleName, u.FullName, u.Login, u.Password
                FROM dbo.Users u
                INNER JOIN dbo.Roles r ON u.RoleId = r.RoleId
                WHERE u.Login = @Login";

            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Login", login.Trim());

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var dbPassword = reader.GetString(5);
                if (string.Equals(dbPassword, password, StringComparison.Ordinal))
                {
                    return new User
                    {
                        UserId = reader.GetInt32(0),
                        RoleId = reader.GetInt32(1),
                        RoleName = reader.GetString(2),
                        FullName = reader.GetString(3),
                        Login = reader.GetString(4),
                        Password = dbPassword
                    };
                }
            }

            return null;
        }

        public List<User> GetAllUsers()
        {
            var result = new List<User>();
            const string sql = @"
                SELECT u.UserId, u.RoleId, r.RoleName, u.FullName, u.Login, u.Password
                FROM dbo.Users u
                INNER JOIN dbo.Roles r ON u.RoleId = r.RoleId
                ORDER BY u.FullName, u.UserId";

            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new User
                {
                    UserId = reader.GetInt32(0),
                    RoleId = reader.GetInt32(1),
                    RoleName = reader.GetString(2),
                    FullName = reader.GetString(3),
                    Login = reader.GetString(4),
                    Password = reader.GetString(5)
                });
            }

            return result;
        }
    }
}
