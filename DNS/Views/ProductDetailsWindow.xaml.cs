using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using DNS.Models;
using DNS.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DNS.Views
{
    public partial class ProductDetailsWindow : Window, INotifyPropertyChanged
    {
        private readonly Product _product;
        private readonly ApplicationDbContext _context;
        public User CurrentUser { get; private set; }
        public string ImageUrl => _product?.ImageUrl;
        public int StockQuantity => _product?.StockQuantity ?? 0;
        public string ProductName => _product?.Name;
        public string ProductDescription => _product?.Description;
        public string ProductPrice => $"{_product?.Price:N0} ₽";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ProductDetailsWindow(Product product, User currentUser = null)
        {
            InitializeComponent();
            _product = product;
            CurrentUser = currentUser;
            _context = new ApplicationDbContext();
            DataContext = this;
            LoadProductDetails();
        }

        private void LoadProductDetails()
        {
            if (_product != null)
            {
                OnPropertyChanged(nameof(ImageUrl));
                OnPropertyChanged(nameof(StockQuantity));
                OnPropertyChanged(nameof(ProductName));
                OnPropertyChanged(nameof(ProductDescription));
                OnPropertyChanged(nameof(ProductPrice));
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser != null)
            {
                try
                {
                    // Проверяем, есть ли уже этот товар в корзине
                    var existingItem = _context.CartItems
                        .FirstOrDefault(ci => ci.UserId == CurrentUser.Id && ci.ProductId == _product.Id);

                    if (existingItem != null)
                    {
                        // Если товар уже есть в корзине, увеличиваем количество
                        existingItem.Quantity++;
                    }
                    else
                    {
                        // Если товара нет в корзине, добавляем новый
                        var cartItem = new CartItem
                        {
                            UserId = CurrentUser.Id,
                            ProductId = _product.Id,
                            Quantity = 1
                        };
                        _context.CartItems.Add(cartItem);
                    }

                    _context.SaveChanges();
                    MessageBox.Show("Товар добавлен в корзину", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении товара в корзину: {ex.Message}", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
