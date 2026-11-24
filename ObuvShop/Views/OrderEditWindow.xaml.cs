using System.Collections.ObjectModel;
using System.Windows;
using ObuvShop.Data;
using ObuvShop.Models;

namespace ObuvShop.Views
{
    public partial class OrderEditWindow : Window
    {
        private readonly OrderRepository _orderRepo = new OrderRepository();
        private readonly UserRepository _userRepo = new UserRepository();
        private readonly ProductRepository _productRepo = new ProductRepository();

        private readonly Order _editingOrder;
        private readonly bool _isEditMode;
        private readonly ObservableCollection<OrderItem> _items = new ObservableCollection<OrderItem>();

        public OrderEditWindow(Order order)
        {
            InitializeComponent();
            _editingOrder = order;
            _isEditMode = order != null;

            OrderItemsGrid.ItemsSource = _items;
            LoadLookups();

            if (_isEditMode)
            {
                Title = $"ООО «Обувь» — Редактирование заказа № {order.OrderId}";
                HeaderTitleText.Text = $"Редактирование заказа № {order.OrderId}";
                FillFields(order);
            }
            else
            {
                Title = "ООО «Обувь» — Добавление заказа";
                HeaderTitleText.Text = "Добавление нового заказа";
                OrderDatePicker.SelectedDate = DateTime.Today;
                DeliveryDatePicker.SelectedDate = DateTime.Today.AddDays(3);
                try
                {
                    PickupCodeTextBox.Text = _orderRepo.GetNextPickupCode().ToString();
                }
                catch
                {
                    PickupCodeTextBox.Text = "911";
                }
            }
        }

        private void LoadLookups()
        {
            StatusComboBox.ItemsSource = _orderRepo.GetStatuses();
            PickupPointComboBox.ItemsSource = _orderRepo.GetPickupPoints();
            ClientComboBox.ItemsSource = _userRepo.GetAllUsers();
            AddProductComboBox.ItemsSource = _productRepo.GetAllProducts();

            if (StatusComboBox.Items.Count > 0) StatusComboBox.SelectedIndex = 0;
            if (PickupPointComboBox.Items.Count > 0) PickupPointComboBox.SelectedIndex = 0;
            if (ClientComboBox.Items.Count > 0) ClientComboBox.SelectedIndex = 0;
            if (AddProductComboBox.Items.Count > 0) AddProductComboBox.SelectedIndex = 0;
        }

        private void FillFields(Order o)
        {
            StatusComboBox.SelectedValue = o.StatusId;
            ClientComboBox.SelectedValue = o.UserId;
            PickupPointComboBox.SelectedValue = o.PickupPointId;
            OrderDatePicker.SelectedDate = o.OrderDate;
            DeliveryDatePicker.SelectedDate = o.DeliveryDate;
            PickupCodeTextBox.Text = o.PickupCode.ToString();

            _items.Clear();
            foreach (var item in o.Items)
            {
                _items.Add(new OrderItem
                {
                    OrderItemId = item.OrderItemId,
                    OrderId = item.OrderId,
                    ProductArticle = item.ProductArticle,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity
                });
            }
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (AddProductComboBox.SelectedItem is not Product selectedProduct)
            {
                MessageBox.Show("Выберите товар из списка.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(AddQtyTextBox.Text.Trim(), out var qty) || qty <= 0)
            {
                MessageBox.Show("Количество должно быть целым положительным числом.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existing = _items.FirstOrDefault(i => i.ProductArticle == selectedProduct.Article);
            if (existing != null)
            {
                existing.Quantity += qty;
                OrderItemsGrid.Items.Refresh();
            }
            else
            {
                _items.Add(new OrderItem
                {
                    ProductArticle = selectedProduct.Article,
                    ProductName = selectedProduct.ProductName,
                    UnitPrice = selectedProduct.FinalPrice,
                    Quantity = qty
                });
            }

            AddQtyTextBox.Text = "1";
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrderItemsGrid.SelectedItem is OrderItem selected)
            {
                _items.Remove(selected);
            }
            else
            {
                MessageBox.Show("Выберите позицию в таблице для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (StatusComboBox.SelectedValue == null || ClientComboBox.SelectedValue == null || PickupPointComboBox.SelectedValue == null)
            {
                MessageBox.Show("Заполните статус, клиента и пункт выдачи.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (OrderDatePicker.SelectedDate == null || DeliveryDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Укажите дату заказа и дату доставки.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DeliveryDatePicker.SelectedDate.Value.Date < OrderDatePicker.SelectedDate.Value.Date)
            {
                MessageBox.Show("Дата доставки не может быть раньше даты заказа.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(PickupCodeTextBox.Text.Trim(), out var pickupCode) || pickupCode <= 0)
            {
                MessageBox.Show("Код получения должен быть положительным целым числом.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_items.Count == 0)
            {
                MessageBox.Show("Добавьте в заказ хотя бы один товар.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var order = new Order
                {
                    OrderId = _isEditMode ? _editingOrder.OrderId : 0,
                    StatusId = (int)StatusComboBox.SelectedValue,
                    UserId = (int)ClientComboBox.SelectedValue,
                    PickupPointId = (int)PickupPointComboBox.SelectedValue,
                    OrderDate = OrderDatePicker.SelectedDate.Value.Date,
                    DeliveryDate = DeliveryDatePicker.SelectedDate.Value.Date,
                    PickupCode = pickupCode,
                    Items = _items.ToList()
                };

                if (_isEditMode)
                    _orderRepo.UpdateOrder(order);
                else
                    _orderRepo.InsertOrder(order);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
