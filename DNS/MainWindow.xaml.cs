using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DNS.Models;
using DNS.Views;
using System.ComponentModel;
using DNS.Data;
using Microsoft.EntityFrameworkCore;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;

namespace DNS
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private User _currentUser;
        private Category _selectedCategory;
        private ObservableCollection<Product> _products;
        private CartWindow _cartWindow;
        private OrderHistoryWindow _orderHistoryWindow;
        private ProfileWindow _profileWindow;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            _currentUser = null; 
            _products = new ObservableCollection<Product>();
            DataContext = this;
            
            OnPropertyChanged(nameof(CurrentUser));
            OnPropertyChanged(nameof(IsUserLoggedIn));
            
            LoadCategories();
            LoadProducts();
        }

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnPropertyChanged(nameof(CurrentUser));
                    OnPropertyChanged(nameof(IsUserLoggedIn));
                }
            }
        }

        public bool IsUserLoggedIn => CurrentUser != null;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LoadCategories()
        {
            using (var context = new ApplicationDbContext())
            {
                var categories = context.Categories.ToList();
                categoriesListBox.ItemsSource = categories;
            }
        }

        private void LoadProducts(int? categoryId = null)
        {
            using (var context = new ApplicationDbContext())
            {
                var query = context.Products
                    .Include(p => p.Category)
                    .AsQueryable();

                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }

                var products = query.ToList();
                Products = new ObservableCollection<Product>(products);

                var categoryName = categoryId.HasValue 
                    ? context.Categories.Find(categoryId.Value)?.Name 
                    : "Все товары";
                productsCountTextBlock.Text = $"{categoryName} ({products.Count})";
            }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
                LoadProducts(_selectedCategory?.Id);
            }
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged(nameof(Products));
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new ProfileWindow(CurrentUser);
            profileWindow.Owner = this;
            if (profileWindow.ShowDialog() == true)
            {
                CurrentUser = null;
            }
        }

        private void CartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cartWindow == null || !_cartWindow.IsLoaded)
            {
                _cartWindow = new CartWindow(CurrentUser);
                _cartWindow.Owner = this;
            }
            _cartWindow.Show();
            _cartWindow.Activate();
        }

        private void OrderHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (_orderHistoryWindow == null || !_orderHistoryWindow.IsLoaded)
            {
                _orderHistoryWindow = new OrderHistoryWindow(CurrentUser);
                _orderHistoryWindow.Owner = this;
            }
            _orderHistoryWindow.Show();
            _orderHistoryWindow.Activate();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser = null;
        }

        private void OpenCart_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser != null)
            {
                if (_cartWindow == null || !_cartWindow.IsVisible)
                {
                    _cartWindow = new CartWindow(CurrentUser);
                    _cartWindow.OrderCompleted += CartWindow_OrderCompleted;
                    _cartWindow.Owner = this;
                    _cartWindow.ShowDialog();
                }
                else
                {
                    _cartWindow.Activate();
                }
            }
        }

        private void OrderHistory_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser != null)
            {
                if (_orderHistoryWindow == null || !_orderHistoryWindow.IsVisible)
                {
                    _orderHistoryWindow = new OrderHistoryWindow(CurrentUser);
                    _orderHistoryWindow.Owner = this;
                    _orderHistoryWindow.ShowDialog();
                }
                else
                {
                    _orderHistoryWindow.Activate();
                }
            }
        }

        private void ViewProductDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Product product)
            {
                var detailsWindow = new ProductDetailsWindow(product, CurrentUser);
                detailsWindow.Owner = this;
                detailsWindow.ShowDialog();
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Product product && CurrentUser != null)
            {
                try
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var currentProduct = context.Products.Find(product.Id);
                        if (currentProduct.StockQuantity <= 0)
                        {
                            MessageBox.Show("Товара нет в наличии", "Предупреждение",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var existingItem = context.CartItems
                            .FirstOrDefault(ci => ci.UserId == CurrentUser.Id && ci.ProductId == product.Id);

                        if (existingItem != null)
                        {
                            if (existingItem.Quantity >= currentProduct.StockQuantity)
                            {
                                MessageBox.Show($"Невозможно добавить больше. На складе осталось {currentProduct.StockQuantity} шт.",
                                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                            existingItem.Quantity++;
                            context.Entry(existingItem).State = EntityState.Modified;
                        }
                        else
                        {
                            var cartItem = new CartItem
                            {
                                UserId = CurrentUser.Id,
                                ProductId = product.Id,
                                Quantity = 1
                            };
                            context.CartItems.Add(cartItem);
                        }

                        context.SaveChanges();
                        LoadProducts(); 
                        MessageBox.Show("Товар добавлен в корзину", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении товара в корзину: {ex.Message}", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchQuery = searchTextBox.Text.ToLower();
            using (var context = new ApplicationDbContext())
            {
                var products = context.Products
                    .Include(p => p.Category)
                    .Where(p => 
                        (SelectedCategory == null || p.CategoryId == SelectedCategory.Id) &&
                        (string.IsNullOrEmpty(searchQuery) || 
                         p.Name.ToLower().Contains(searchQuery) || 
                         p.Description.ToLower().Contains(searchQuery)))
                    .ToList();

                Products = new ObservableCollection<Product>(products);
            }
        }

        private void CategoriesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (categoriesListBox.SelectedItem is Category category)
            {
                SelectedCategory = category;
            }
        }

        private void CartWindow_OrderCompleted(object sender, EventArgs e)
        {
            LoadProducts(); 
        }

        private void OnAllProductsClick(object sender, RoutedEventArgs e)
        {
            categoriesListBox.SelectedItem = null;
            LoadProducts();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Owner = this;
            if (loginWindow.ShowDialog() == true && loginWindow.User != null)
            {
                CurrentUser = loginWindow.User;
                OnPropertyChanged(nameof(CurrentUser));

                // Проверяем роль пользователя
                if (CurrentUser.IsAdmin)
                {
                    var adminWindow = new AdminWindow();
                    adminWindow.Show();
                    this.Hide(); // Скрываем главное окно

                    // При закрытии окна администратора
                    adminWindow.Closed += (s, args) =>
                    {
                        CurrentUser = null;
                        this.Show(); // Показываем главное окно снова
                    };
                }
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Owner = this;
            if (registerWindow.ShowDialog() == true)
            {
                var loginWindow = new LoginWindow();
                loginWindow.UsernameTextBox.Text = registerWindow.Username;
                loginWindow.PasswordBox.Password = registerWindow.Password;
                loginWindow.Owner = this;
                if (loginWindow.ShowDialog() == true && loginWindow.User != null)
                {
                    CurrentUser = loginWindow.User;
                    OnPropertyChanged(nameof(CurrentUser));
                }
            }
        }
    }
}