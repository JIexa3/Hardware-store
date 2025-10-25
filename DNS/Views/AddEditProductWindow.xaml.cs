using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using DNS.Data;
using DNS.Models;
using DNS.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace DNS.Views
{
    public partial class AddEditProductWindow : Window
    {
        private readonly int? _productId;
        private string? _selectedImagePath;
        private ProductViewModel _productViewModel;

        public AddEditProductWindow(Product? product = null)
        {
            InitializeComponent();
            _productId = product?.Id;

            LoadCategories();
            if (_productId.HasValue)
            {
                LoadProductData();
                Title = "Редактирование товара";
            }
            else
            {
                _productViewModel = new ProductViewModel(new Product());
                DataContext = _productViewModel;
                Title = "Новый товар";
            }
        }

        private void LoadCategories()
        {
            using (var context = new ApplicationDbContext())
            {
                var categories = context.Categories
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .ToList();
                CategoryComboBox.ItemsSource = categories;
            }
        }

        private void LoadProductData()
        {
            if (!_productId.HasValue) return;

            using (var context = new ApplicationDbContext())
            {
                var product = context.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .FirstOrDefault(p => p.Id == _productId);

                if (product != null)
                {
                    _productViewModel = new ProductViewModel(product);
                    DataContext = _productViewModel;

                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        LoadImage(product.ImageUrl);
                    }
                }
            }
        }

        private void LoadImage(string imagePath)
        {
            try
            {
                var fullPath = imagePath;
                if (imagePath.StartsWith("/"))
                {
                    fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath.TrimStart('/'));
                }

                if (File.Exists(fullPath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.CreateOptions = BitmapCreateOptions.None;
                    using (var stream = File.OpenRead(fullPath))
                    {
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        if (bitmap.CanFreeze) bitmap.Freeze();
                    }
                    ProductImage.Source = bitmap;
                    _selectedImagePath = imagePath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif|Все файлы|*.*",
                Title = "Выберите изображение товара"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var fileName = Path.GetFileName(openFileDialog.FileName);
                    var imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    var productsDirectory = Path.Combine(imagesDirectory, "Products");
                    
                    Directory.CreateDirectory(imagesDirectory);
                    Directory.CreateDirectory(productsDirectory);

                    var uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{fileName}";
                    var targetPath = Path.Combine(productsDirectory, uniqueFileName);

                    File.Copy(openFileDialog.FileName, targetPath);

                    _selectedImagePath = $"/Images/Products/{uniqueFileName}";
                    LoadImage(_selectedImagePath);
                    _productViewModel.ImageUrl = _selectedImagePath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при выборе изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_productViewModel.Name))
                {
                    MessageBox.Show("Введите название товара", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedCategory = CategoryComboBox.SelectedItem as Category;
                if (selectedCategory == null)
                {
                    MessageBox.Show("Выберите категорию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _productViewModel.CategoryId = selectedCategory.Id;

                using (var context = new ApplicationDbContext())
                {
                    var product = _productViewModel.ToModel();

                    if (_productId.HasValue)
                    {
                        var existingProduct = context.Products.Find(_productId.Value);
                        if (existingProduct != null)
                        {
                            context.Entry(existingProduct).CurrentValues.SetValues(product);
                            existingProduct.CategoryId = selectedCategory.Id;
                        }
                    }
                    else
                    {
                        product.CategoryId = selectedCategory.Id;
                        context.Products.Add(product);
                    }

                    context.SaveChanges();
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
