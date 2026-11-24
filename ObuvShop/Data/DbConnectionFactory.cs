using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace ObuvShop.Data
{
    public static class DbConnectionFactory
    {
        private const string DefaultConnectionString =
            @"Server=.\SQLEXPRESS;Database=ObuvDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private static string _connectionString;

        private static string SettingsFilePath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_connectionString))
                {
                    _connectionString = LoadConnectionString();
                }
                return _connectionString;
            }
        }

        private static string LoadConnectionString()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("ConnectionString", out var prop))
                    {
                        var cs = prop.GetString();
                        if (!string.IsNullOrWhiteSpace(cs))
                            return cs;
                    }
                }
            }
            catch
            {
            }

            return DefaultConnectionString;
        }

        public static void SaveConnectionString(string connectionString)
        {
            _connectionString = connectionString;
            var obj = new Dictionary<string, string>
            {
                ["ConnectionString"] = connectionString
            };
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }

        public static SqlConnection CreateConnection() => new SqlConnection(ConnectionString);

        public static bool TestConnection(string connectionString, out string errorMessage)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                if (builder.ConnectTimeout > 5)
                    builder.ConnectTimeout = 5;

                using var conn = new SqlConnection(builder.ConnectionString);
                conn.Open();
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Users", conn);
                cmd.ExecuteScalar();
                errorMessage = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
