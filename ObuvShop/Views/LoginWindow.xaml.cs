using System.Windows;
using System.Windows.Input;
using ObuvShop.Data;
using ObuvShop.Models;

namespace ObuvShop.Views
{
    public partial class LoginWindow : Window
    {
        private readonly UserRepository _userRepo = new UserRepository();

        public LoginWindow()
        {
            InitializeComponent();
            LoginTextBox.Focus();
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AttemptLogin();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            AttemptLogin();
        }

        private void AttemptLogin()
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;

            var login = LoginTextBox.Text.Trim();
            var password = PasswordBoxInput.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите логин и пароль.");
                return;
            }

            try
            {
                var user = _userRepo.Authenticate(login, password);
                if (user == null)
                {
                    ShowError("Неверный логин или пароль. Проверьте введённые данные и повторите попытку.");
                    return;
                }

                OpenMainWindow(user);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось подключиться к базе данных MS SQL Server.\n\n" +
                    "Проверьте, что скрипт Database\\01_CreateAndSeed_ObuvDB.sql выполнен в SSMS, " +
                    "а строка подключения указана верно (кнопка «Настройка БД (SSMS)»).\n\n" +
                    $"Подробности: {ex.Message}",
                    "Ошибка подключения к БД",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!DbConnectionFactory.TestConnection(DbConnectionFactory.ConnectionString, out var err))
                {
                    MessageBox.Show(
                        "Не удалось подключиться к базе данных MS SQL Server.\n\n" +
                        "Нажмите «Настройка БД (SSMS)» и укажите имя вашего сервера SQL Server.\n\n" +
                        $"Подробности: {err}",
                        "Ошибка подключения к БД",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                OpenMainWindow(User.CreateGuest());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перехода в режим гостя: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenMainWindow(User user)
        {
            var mainWindow = new MainWindow(user);
            mainWindow.Show();
            Close();
        }

        private void DbSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new ConnectionSettingsWindow { Owner = this };
            win.ShowDialog();
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
    }
}
