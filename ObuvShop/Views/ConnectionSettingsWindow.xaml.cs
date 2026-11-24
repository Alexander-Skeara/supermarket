using System.Windows;
using System.Windows.Media;
using Microsoft.Data.SqlClient;
using ObuvShop.Data;

namespace ObuvShop.Views
{
    public partial class ConnectionSettingsWindow : Window
    {
        public ConnectionSettingsWindow()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(DbConnectionFactory.ConnectionString);
                ServerTextBox.Text = string.IsNullOrWhiteSpace(builder.DataSource) ? @".\SQLEXPRESS" : builder.DataSource;
                DatabaseTextBox.Text = string.IsNullOrWhiteSpace(builder.InitialCatalog) ? "ObuvDB" : builder.InitialCatalog;
                TrustedConnectionCheckBox.IsChecked = builder.IntegratedSecurity;
                SqlUserTextBox.Text = builder.UserID;
                SqlPasswordBox.Password = builder.Password;
                SqlAuthPanel.IsEnabled = !builder.IntegratedSecurity;
            }
            catch
            {
                ServerTextBox.Text = @".\SQLEXPRESS";
                DatabaseTextBox.Text = "ObuvDB";
                TrustedConnectionCheckBox.IsChecked = true;
                SqlAuthPanel.IsEnabled = false;
            }
        }

        private void AuthMode_Changed(object sender, RoutedEventArgs e)
        {
            if (SqlAuthPanel != null)
            {
                SqlAuthPanel.IsEnabled = TrustedConnectionCheckBox.IsChecked != true;
            }
        }

        private string BuildConnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = ServerTextBox.Text.Trim(),
                InitialCatalog = DatabaseTextBox.Text.Trim(),
                IntegratedSecurity = TrustedConnectionCheckBox.IsChecked == true,
                TrustServerCertificate = true
            };

            if (!builder.IntegratedSecurity)
            {
                builder.UserID = SqlUserTextBox.Text.Trim();
                builder.Password = SqlPasswordBox.Password;
            }

            return builder.ConnectionString;
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            var cs = BuildConnectionString();
            if (DbConnectionFactory.TestConnection(cs, out var err))
            {
                StatusTextBlock.Foreground = Brushes.DarkGreen;
                StatusTextBlock.Text = "Подключение к базе данных ObuvDB успешно установлено!";
            }
            else
            {
                StatusTextBlock.Foreground = Brushes.DarkRed;
                StatusTextBlock.Text = $"Ошибка подключения: {err}";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ServerTextBox.Text) || string.IsNullOrWhiteSpace(DatabaseTextBox.Text))
            {
                MessageBox.Show("Укажите сервер и имя базы данных.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DbConnectionFactory.SaveConnectionString(BuildConnectionString());
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить настройки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
