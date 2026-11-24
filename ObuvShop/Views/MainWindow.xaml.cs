using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ObuvShop.Data;
using ObuvShop.Models;

namespace ObuvShop.Views
{
    public partial class MainWindow : Window
    {
        private readonly User _currentUser;
        private readonly ProductRepository _productRepo = new ProductRepository();
        private readonly OrderRepository _orderRepo = new OrderRepository();

        private List<Product> _allProducts = new List<Product>();
        private List<Order> _allOrders = new List<Order>();
        private bool _isInitializing = true;

        public MainWindow(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser ?? User.CreateGuest();
            ConfigurePermissions();
            LoadSuppliersFilter();
            LoadProducts();
            if (_currentUser.CanViewOrders)
            {
                LoadOrders();
            }
            _isInitializing = false;
            ApplyProductFilters();
            ShowProductsSection();
        }

        // вроде работает, но я где то накосячил
        private void ConfigurePermissions()
        {
            UserFullNameText.Text = _currentUser.FullName;
            UserRoleText.Text = _currentUser.RoleName;

            FilterPanel.Visibility = _currentUser.CanSearchAndFilter ? Visibility.Visible : Visibility.Collapsed;
            OrdersNavButton.Visibility = _currentUser.CanViewOrders ? Visibility.Visible : Visibility.Collapsed;
            ProductsAdminPanel.Visibility = _currentUser.CanEditProducts ? Visibility.Visible : Visibility.Collapsed;
            OrdersAdminPanel.Visibility = _currentUser.CanEditOrders ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadSuppliersFilter()
        {
            try
            {
                var items = new List<LookupItem>
                {
                    new LookupItem { Id = 0, Name = "Все поставщики" }
                };
                items.AddRange(_productRepo.GetSuppliers());
                SupplierFilterComboBox.ItemsSource = items;
                SupplierFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поставщиков: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                _allProducts = _productRepo.GetAllProducts();
                ApplyProductFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров из БД: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrders()
        {
            try
            {
                _allOrders = _orderRepo.GetAllOrders();
                OrdersListBox.ItemsSource = _allOrders;
                OrdersCountText.Text = $"Всего заказов: {_allOrders.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов из БД: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyProductFilters()
        {
            if (_isInitializing)
                return;

            IEnumerable<Product> query = _allProducts;

            if (_currentUser.CanSearchAndFilter)
            {
                var search = SearchTextBox.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(p =>
                        (p.Article?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (p.ProductName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (p.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (p.ManufacturerName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (p.SupplierName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (p.CategoryName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                if (SupplierFilterComboBox.SelectedItem is LookupItem selectedSupplier && selectedSupplier.Id > 0)
                {
                    query = query.Where(p => p.SupplierId == selectedSupplier.Id);
                }

                switch (SortComboBox.SelectedIndex)
                {
                    case 1:
                        query = query.OrderBy(p => p.StockQuantity);
                        break;
                    case 2:
                        query = query.OrderByDescending(p => p.StockQuantity);
                        break;
                    case 3:
                        query = query.OrderBy(p => p.FinalPrice);
                        break;
                    case 4:
                        query = query.OrderByDescending(p => p.FinalPrice);
                        break;
                }
            }

            var filtered = query.ToList();
            ProductsListBox.ItemsSource = filtered;
            ProductsCountText.Text = $"Показано: {filtered.Count} из {_allProducts.Count}";
        }

        private void FilterControls_Changed(object sender, EventArgs e)
        {
            ApplyProductFilters();
        }

        private void ResetFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            _isInitializing = true;
            SearchTextBox.Text = string.Empty;
            SupplierFilterComboBox.SelectedIndex = 0;
            SortComboBox.SelectedIndex = 0;
            _isInitializing = false;
            ApplyProductFilters();
        }

        private void ShowProductsSection()
        {
            ProductsSection.Visibility = Visibility.Visible;
            OrdersSection.Visibility = Visibility.Collapsed;
            Title = "ООО «Обувь» — Каталог товаров";
            SectionSubtitleText.Text = "Каталог товаров";
            ProductsNavButton.Style = (Style)FindResource("AccentButtonStyle");
            OrdersNavButton.Style = (Style)FindResource(typeof(Button));
        }

        private void ShowOrdersSection()
        {
            if (!_currentUser.CanViewOrders)
                return;

            LoadOrders();
            ProductsSection.Visibility = Visibility.Collapsed;
            OrdersSection.Visibility = Visibility.Visible;
            Title = "ООО «Обувь» — Заказы";
            SectionSubtitleText.Text = "Управление заказами";
            OrdersNavButton.Style = (Style)FindResource("AccentButtonStyle");
            ProductsNavButton.Style = (Style)FindResource(typeof(Button));
        }

        private void ProductsNavButton_Click(object sender, RoutedEventArgs e) => ShowProductsSection();
        private void OrdersNavButton_Click(object sender, RoutedEventArgs e) => ShowOrdersSection();

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        // CRUD Товаров (Администратор)
        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentUser.CanEditProducts) return;
            var win = new ProductEditWindow(null) { Owner = this };
            if (win.ShowDialog() == true)
            {
                LoadProducts();
            }
        }

        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedProduct();
        }

        private void ProductsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_currentUser.CanEditProducts && ProductsListBox.SelectedItem is Product)
            {
                EditSelectedProduct();
            }
        }

        private void EditSelectedProduct()
        {
            if (!_currentUser.CanEditProducts) return;
            if (ProductsListBox.SelectedItem is not Product selected)
            {
                MessageBox.Show("Выберите товар для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new ProductEditWindow(selected) { Owner = this };
            if (win.ShowDialog() == true)
            {
                LoadProducts();
            }
        }

        private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentUser.CanEditProducts) return;
            if (ProductsListBox.SelectedItem is not Product selected)
            {
                MessageBox.Show("Выберите товар для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Вы действительно хотите удалить товар «{selected.ProductName}» (артикул {selected.Article})?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _productRepo.DeleteProduct(selected.Article);
                LoadProducts();
                MessageBox.Show("Товар успешно удалён.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Удаление невозможно", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении товара: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // CRUD Заказов (Администратор)
        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentUser.CanEditOrders) return;
            var win = new OrderEditWindow(null) { Owner = this };
            if (win.ShowDialog() == true)
            {
                LoadOrders();
            }
        }

        private void EditOrderButton_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedOrder();
        }

        private void OrdersListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_currentUser.CanEditOrders && OrdersListBox.SelectedItem is Order)
            {
                EditSelectedOrder();
            }
        }

        private void EditSelectedOrder()
        {
            if (!_currentUser.CanEditOrders) return;
            if (OrdersListBox.SelectedItem is not Order selected)
            {
                MessageBox.Show("Выберите заказ для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new OrderEditWindow(selected) { Owner = this };
            if (win.ShowDialog() == true)
            {
                LoadOrders();
            }
        }

        private void DeleteOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentUser.CanEditOrders) return;
            if (OrdersListBox.SelectedItem is not Order selected)
            {
                MessageBox.Show("Выберите заказ для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Удалить заказ № {selected.OrderId} (клиент: {selected.ClientFullName})?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _orderRepo.DeleteOrder(selected.OrderId);
                LoadOrders();
                MessageBox.Show("Заказ успешно удалён.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
