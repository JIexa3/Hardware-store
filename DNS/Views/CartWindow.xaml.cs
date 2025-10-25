using System;
using System.Linq;
using System.Windows;
using DNS.Models;
using DNS.Data;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Windows.Controls;

namespace DNS.Views
{
    public partial class CartWindow : Window, INotifyPropertyChanged
    {
        private readonly User _currentUser;
        private ObservableCollection<CartItem> _cartItems;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler OrderCompleted;

        public ObservableCollection<CartItem> CartItems
        {
            get => _cartItems;
            set
            {
                _cartItems = value;
                OnPropertyChanged(nameof(CartItems));
                UpdateTotalPrice();
            }
        }

        public CartWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            DataContext = this;
            LoadCartItems();
        }

        private void LoadCartItems()
        {
            using (var context = new ApplicationDbContext())
            {
                var items = context.CartItems
                    .Include(ci => ci.Product)
                    .Where(ci => ci.UserId == _currentUser.Id)
                    .ToList();

                CartItems = new ObservableCollection<CartItem>(items);
            }
        }

        private void UpdateTotalPrice()
        {
            if (CartItems != null)
            {
                decimal total = CartItems.Sum(item => item.Product.Price * item.Quantity);
                totalPriceTextBlock.Text = $"{total:N0} ₽";
            }
            else
            {
                totalPriceTextBlock.Text = "0 ₽";
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartItem cartItem)
            {
                using (var context = new ApplicationDbContext())
                {
                    var product = context.Products.Find(cartItem.ProductId);
                    if (product == null)
                    {
                        MessageBox.Show("Товар не найден", "Ошибка", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var item = context.CartItems.Find(cartItem.Id);
                    if (item == null)
                    {
                        MessageBox.Show("Товар не найден в корзине", "Ошибка", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Проверяем наличие товара на складе
                    if (item.Quantity >= product.StockQuantity)
                    {
                        MessageBox.Show($"Невозможно добавить больше. На складе осталось {product.StockQuantity} шт.", 
                            "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    item.Quantity++;
                    context.Entry(item).State = EntityState.Modified;
                    context.SaveChanges();
                    LoadCartItems();
                }
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartItem cartItem)
            {
                using (var context = new ApplicationDbContext())
                {
                    var item = context.CartItems.Find(cartItem.Id);
                    if (item != null && item.Quantity > 1)
                    {
                        item.Quantity--;
                        context.Entry(item).State = EntityState.Modified;
                        context.SaveChanges();
                        LoadCartItems();
                    }
                }
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartItem cartItem)
            {
                if (MessageBox.Show("Вы действительно хотите удалить этот товар из корзины?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var item = context.CartItems.Find(cartItem.Id);
                        if (item != null)
                        {
                            context.CartItems.Remove(item);
                            context.SaveChanges();
                            LoadCartItems();
                        }
                    }
                }
            }
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CartItems.Any())
                {
                    MessageBox.Show("Корзина пуста", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var context = new ApplicationDbContext())
                {
                    // Проверяем наличие всех товаров перед оформлением заказа
                    foreach (var cartItem in CartItems)
                    {
                        var product = context.Products.Find(cartItem.ProductId);
                        if (product == null || product.StockQuantity < cartItem.Quantity)
                        {
                            MessageBox.Show($"Недостаточно товара {cartItem.Product.Name} на складе. " +
                                          $"Доступно: {product?.StockQuantity ?? 0} шт.", 
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    // Создаем заказ
                    var order = new Order
                    {
                        UserId = _currentUser.Id,
                        OrderDate = DateTime.Now,
                        TotalAmount = CartItems.Sum(item => item.Product.Price * item.Quantity),
                        Status = OrderStatus.Новый
                    };
                    context.Orders.Add(order);
                    context.SaveChanges(); // Сохраняем заказ, чтобы получить Id

                    // Добавляем элементы заказа и обновляем количество товаров
                    foreach (var cartItem in CartItems)
                    {
                        var product = context.Products.Find(cartItem.ProductId);
                        
                        // Создаем элемент заказа
                        var orderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = cartItem.ProductId,
                            Quantity = cartItem.Quantity,
                            Price = cartItem.Product.Price
                        };
                        context.OrderItems.Add(orderItem);

                        // Обновляем количество товара на складе
                        product.StockQuantity -= cartItem.Quantity;
                        context.Entry(product).State = EntityState.Modified;
                    }

                    // Очищаем корзину
                    var cartItems = context.CartItems.Where(ci => ci.UserId == _currentUser.Id);
                    context.CartItems.RemoveRange(cartItems);

                    // Сохраняем все изменения
                    context.SaveChanges();

                    MessageBox.Show("Заказ успешно оформлен!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    OrderCompleted?.Invoke(this, EventArgs.Empty);
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
