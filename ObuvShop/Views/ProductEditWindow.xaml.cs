using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ObuvShop.Data;
using ObuvShop.Models;

namespace ObuvShop.Views
{
    public partial class ProductEditWindow : Window
    {
        private readonly ProductRepository _productRepo = new ProductRepository();
        private readonly Product _editingProduct;
        private readonly bool _isEditMode;
        private string _currentPhotoFileName;
        private string _selectedNewSourcePhotoPath;

        public ProductEditWindow(Product product)
        {
            InitializeComponent();
            _editingProduct = product;
            _isEditMode = product != null;

            LoadLookups();

            if (_isEditMode)
            {
                Title = $"ООО «Обувь» — Редактирование товара ({product.Article})";
                HeaderTitleText.Text = $"Редактирование товара: {product.Article}";
                FillFields(product);
            }
            else
            {
                Title = "ООО «Обувь» — Добавление товара";
                HeaderTitleText.Text = "Добавление нового товара";
                LoadPreview(null);
            }
        }

        private void LoadLookups()
        {
            CategoryComboBox.ItemsSource = _productRepo.GetCategories();
            UnitComboBox.ItemsSource = _productRepo.GetUnits();
            ManufacturerComboBox.ItemsSource = _productRepo.GetManufacturers();
            SupplierComboBox.ItemsSource = _productRepo.GetSuppliers();

            if (CategoryComboBox.Items.Count > 0) CategoryComboBox.SelectedIndex = 0;
            if (UnitComboBox.Items.Count > 0) UnitComboBox.SelectedIndex = 0;
            if (ManufacturerComboBox.Items.Count > 0) ManufacturerComboBox.SelectedIndex = 0;
            if (SupplierComboBox.Items.Count > 0) SupplierComboBox.SelectedIndex = 0;
        }

        private void FillFields(Product p)
        {
            ArticleTextBox.Text = p.Article;
            ArticleTextBox.IsReadOnly = true;
            ArticleTextBox.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));

            NameTextBox.Text = p.ProductName;
            CategoryComboBox.SelectedValue = p.CategoryId;
            UnitComboBox.SelectedValue = p.UnitId;
            ManufacturerComboBox.SelectedValue = p.ManufacturerId;
            SupplierComboBox.SelectedValue = p.SupplierId;
            PriceTextBox.Text = p.Price.ToString("0.##", CultureInfo.InvariantCulture);
            DiscountTextBox.Text = p.Discount.ToString();
            StockTextBox.Text = p.StockQuantity.ToString();
            DescriptionTextBox.Text = p.Description;

            _currentPhotoFileName = p.PhotoPath;
            LoadPreview(_currentPhotoFileName);
        }

        private void LoadPreview(string photoPathOrFileName)
        {
            try
            {
                string fullPath = null;
                if (!string.IsNullOrWhiteSpace(photoPathOrFileName))
                {
                    fullPath = Path.IsPathRooted(photoPathOrFileName)
                        ? photoPathOrFileName
                        : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", photoPathOrFileName);
                }

                if (!string.IsNullOrWhiteSpace(fullPath) && File.Exists(fullPath))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(fullPath, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bmp.EndInit();
                    PreviewImage.Source = bmp;
                    PhotoFileNameText.Text = Path.GetFileName(fullPath);
                }
                else
                {
                    PreviewImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/picture.png", UriKind.Absolute));
                    PhotoFileNameText.Text = "Заглушка (picture.png)";
                }
            }
            catch
            {
                PreviewImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/picture.png", UriKind.Absolute));
                PhotoFileNameText.Text = "Заглушка (picture.png)";
            }
        }

        private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Выберите изображение товара",
                Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (dlg.ShowDialog() == true)
            {
                _selectedNewSourcePhotoPath = dlg.FileName;
                LoadPreview(_selectedNewSourcePhotoPath);
            }
        }

        private void ClearPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedNewSourcePhotoPath = null;
            _currentPhotoFileName = null;
            LoadPreview(null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var article = ArticleTextBox.Text.Trim();
            var name = NameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(article))
            {
                MessageBox.Show("Укажите артикул товара.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Укажите наименование товара.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var priceRaw = PriceTextBox.Text.Trim().Replace(',', '.');
            if (!decimal.TryParse(priceRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) || price < 0)
            {
                MessageBox.Show("Цена должна быть неотрицательным числом.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(DiscountTextBox.Text.Trim(), out var discount) || discount < 0 || discount > 100)
            {
                MessageBox.Show("Скидка должна быть целым числом от 0 до 100.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(StockTextBox.Text.Trim(), out var stock) || stock < 0)
            {
                MessageBox.Show("Количество на складе должно быть целым неотрицательным числом.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CategoryComboBox.SelectedValue == null || UnitComboBox.SelectedValue == null ||
                ManufacturerComboBox.SelectedValue == null || SupplierComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите значения во всех справочных полях.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (!_isEditMode && _productRepo.ArticleExists(article))
                {
                    MessageBox.Show($"Товар с артикулом «{article}» уже существует в базе данных.",
                        "Дубликат артикула", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string finalPhotoFileName = _currentPhotoFileName;
                if (!string.IsNullOrWhiteSpace(_selectedNewSourcePhotoPath) && File.Exists(_selectedNewSourcePhotoPath))
                {
                    var imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    Directory.CreateDirectory(imagesDir);
                    var ext = Path.GetExtension(_selectedNewSourcePhotoPath);
                    var safeName = $"{article}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                    var destPath = Path.Combine(imagesDir, safeName);
                    File.Copy(_selectedNewSourcePhotoPath, destPath, true);
                    finalPhotoFileName = safeName;
                }

                var model = new Product
                {
                    Article = article,
                    ProductName = name,
                    CategoryId = (int)CategoryComboBox.SelectedValue,
                    UnitId = (int)UnitComboBox.SelectedValue,
                    ManufacturerId = (int)ManufacturerComboBox.SelectedValue,
                    SupplierId = (int)SupplierComboBox.SelectedValue,
                    Price = Math.Round(price, 2),
                    Discount = discount,
                    StockQuantity = stock,
                    Description = DescriptionTextBox.Text?.Trim(),
                    PhotoPath = finalPhotoFileName
                };

                if (_isEditMode)
                    _productRepo.UpdateProduct(model);
                else
                    _productRepo.InsertProduct(model);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения товара: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
