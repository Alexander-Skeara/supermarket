namespace ObuvShop.Models
{
    public class User
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public bool IsGuest => UserId == 0;
        public bool IsAdmin => string.Equals(RoleName, "Администратор", StringComparison.OrdinalIgnoreCase);
        public bool IsManager => string.Equals(RoleName, "Менеджер", StringComparison.OrdinalIgnoreCase);
        public bool IsClient => string.Equals(RoleName, "Авторизированный клиент", StringComparison.OrdinalIgnoreCase);

        public bool CanSearchAndFilter => IsAdmin || IsManager;
        public bool CanViewOrders => IsAdmin || IsManager;
        public bool CanEditProducts => IsAdmin;
        public bool CanEditOrders => IsAdmin;

        public static User CreateGuest() => new User
        {
            UserId = 0,
            RoleId = 0,
            RoleName = "Гость",
            FullName = "Гость",
            Login = string.Empty,
            Password = string.Empty
        };

        public override string ToString() => $"{FullName} ({RoleName})";
    }
}
