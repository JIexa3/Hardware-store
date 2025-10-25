using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using DNS.Data;
using DNS.Models;
using DNS.ViewModels;
using Microsoft.EntityFrameworkCore;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;

namespace DNS.Views
{
    public partial class AdminWindow : Window
    {
        private ObservableCollection<ProductViewModel> Products { get; set; }
        private ObservableCollection<Category> Categories { get; set; }
        private ObservableCollection<Order> Orders { get; set; }

        // Данные для графиков
        public SeriesCollection SalesChartData { get; set; }
        public string[] SalesChartLabels { get; set; }
        public Func<double, string> SalesYFormatter { get; set; }

        public SeriesCollection CategoriesChartData { get; set; }

        public SeriesCollection MonthlySalesChartData { get; set; }
        public string[] MonthlySalesChartLabels { get; set; }
        public Func<double, string> MonthlySalesYFormatter { get; set; }

        public AdminWindow()
        {
            InitializeComponent();
            DataContext = this;

            Products = new ObservableCollection<ProductViewModel>();
            Categories = new ObservableCollection<Category>();
            Orders = new ObservableCollection<Order>();

            ProductsDataGrid.ItemsSource = Products;
            CategoriesDataGrid.ItemsSource = Categories;
            OrdersDataGrid.ItemsSource = Orders;

            // Инициализация графиков
            InitializeCharts();

            LoadData();
            UpdateStatistics();
            UpdateCharts();
        }

        private void InitializeCharts()
        {
            // Инициализация коллекций для графиков
            SalesChartData = new SeriesCollection();
            CategoriesChartData = new SeriesCollection();
            MonthlySalesChartData = new SeriesCollection();

            // Форматтер для денежных значений
            SalesYFormatter = value => value.ToString("N0") + " ₽";
            MonthlySalesYFormatter = value => value.ToString("N0") + " ₽";
        }

        private void UpdateCharts()
        {
            using (var context = new ApplicationDbContext())
            {
                // График продаж за последние 7 дней
                var last7Days = Enumerable.Range(0, 7)
                    .Select(i => DateTime.Today.AddDays(-i))
                    .Reverse()
                    .ToList();

                var dailySales = last7Days.Select(date =>
                {
                    var nextDay = date.AddDays(1);
                    return new
                    {
                        Date = date,
                        Total = context.Orders
                            .Where(o => o.OrderDate >= date && o.OrderDate < nextDay)
                            .Sum(o => (decimal?)o.TotalAmount) ?? 0
                    };
                }).ToList();

                SalesChartData.Clear();
                SalesChartData.Add(new LineSeries
                {
                    Title = "Продажи",
                    Values = new ChartValues<decimal>(dailySales.Select(x => x.Total)),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 15
                });

                SalesChartLabels = dailySales.Select(x => x.Date.ToString("dd.MM")).ToArray();

                // График популярных категорий
                var categoryStats = context.OrderItems
                    .Include(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                    .GroupBy(oi => oi.Product.Category.Name)
                    .Select(g => new
                    {
                        Category = g.Key,
                        Total = g.Sum(oi => oi.Quantity * oi.Product.Price)
                    })
                    .OrderByDescending(x => x.Total)
                    .Take(5)
                    .ToList();

                CategoriesChartData.Clear();
                foreach (var stat in categoryStats)
                {
                    CategoriesChartData.Add(new PieSeries
                    {
                        Title = stat.Category,
                        Values = new ChartValues<decimal> { stat.Total },
                        DataLabels = true
                    });
                }

                // График продаж по месяцам
                var last12Months = Enumerable.Range(0, 12)
                    .Select(i => DateTime.Today.AddMonths(-i))
                    .Reverse()
                    .ToList();

                var monthlySales = last12Months.Select(date =>
                {
                    var startOfMonth = new DateTime(date.Year, date.Month, 1);
                    var endOfMonth = startOfMonth.AddMonths(1);
                    return new
                    {
                        Month = date,
                        Total = context.Orders
                            .Where(o => o.OrderDate >= startOfMonth && o.OrderDate < endOfMonth)
                            .Sum(o => (decimal?)o.TotalAmount) ?? 0
                    };
                }).ToList();

                MonthlySalesChartData.Clear();
                MonthlySalesChartData.Add(new ColumnSeries
                {
                    Title = "Продажи",
                    Values = new ChartValues<decimal>(monthlySales.Select(x => x.Total))
                });

                MonthlySalesChartLabels = monthlySales.Select(x => x.Month.ToString("MM.yyyy")).ToArray();
            }
        }

        private void LoadData()
        {
            LoadProducts();
            LoadCategories();
            LoadOrders();
        }

        private void LoadProducts()
        {
            using (var context = new ApplicationDbContext())
            {
                var products = context.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .ToList();

                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(new ProductViewModel(product));
                }
            }
        }

        private void LoadCategories()
        {
            using (var context = new ApplicationDbContext())
            {
                var categories = context.Categories
                    .AsNoTracking()
                    .ToList();

                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
        }

        private void LoadOrders()
        {
            using (var context = new ApplicationDbContext())
            {
                var orders = context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                            .ThenInclude(p => p.Category)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                Orders.Clear();
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }
            }
        }

        private void UpdateStatistics()
        {
            using (var context = new ApplicationDbContext())
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // Количество заказов за сегодня
                var todayOrders = context.Orders
                    .AsNoTracking()
                    .Count(o => o.OrderDate >= today && o.OrderDate < tomorrow);
                TodayOrdersCount.Text = todayOrders.ToString();

                // Общее количество товаров
                var totalProducts = context.Products
                    .AsNoTracking()
                    .Count();
                TotalProducts.Text = totalProducts.ToString();

                // Общая выручка за сегодня
                var todayRevenue = context.Orders
                    .AsNoTracking()
                    .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow)
                    .Sum(o => o.TotalAmount);
                TodayRevenue.Text = $"{todayRevenue:N0} ₽";

                // Количество активных пользователей
                var thirtyDaysAgo = DateTime.Now.AddDays(-30);
                var activeUsers = context.Users
                    .AsNoTracking()
                    .Count(u => u.RegisterDate >= thirtyDaysAgo);
                ActiveUsers.Text = activeUsers.ToString();
            }
        }

        private void OnAddProductButtonClick(object sender, RoutedEventArgs e)
        {
            var addProductWindow = new AddEditProductWindow();
            if (addProductWindow.ShowDialog() == true)
            {
                LoadProducts();
                UpdateStatistics();
            }
        }

        private void OnEditProductButtonClick(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is ProductViewModel selectedProduct)
            {
                var editProductWindow = new AddEditProductWindow(selectedProduct.ToModel());
                if (editProductWindow.ShowDialog() == true)
                {
                    LoadProducts();
                }
            }
        }

        private void OnDeleteProductButtonClick(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is ProductViewModel selectedProduct)
            {
                var result = MessageBox.Show(
                    "Вы уверены, что хотите удалить этот товар?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new ApplicationDbContext())
                        {
                            var product = context.Products.Find(selectedProduct.Id);
                            if (product != null)
                            {
                                context.Products.Remove(product);
                                context.SaveChanges();
                                LoadProducts();
                                UpdateStatistics();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Ошибка при удалении товара: {ex.Message}",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }

        private void OnAddCategoryButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new TextDialog("Новая категория", "Введите название категории:");
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ResponseText))
            {
                try
                {
                    using (var context = new ApplicationDbContext())
                    {
                        // Проверяем, существует ли уже категория с таким именем
                        var existingCategory = context.Categories
                            .FirstOrDefault(c => c.Name.ToLower() == dialog.ResponseText.ToLower());

                        if (existingCategory != null)
                        {
                            MessageBox.Show(
                                "Категория с таким названием уже существует",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        var category = new Category 
                        { 
                            Name = dialog.ResponseText.Trim(),
                            Description = "" // Добавляем пустое описание
                        };

                        context.Categories.Add(category);
                        context.SaveChanges();
                        LoadCategories();

                        MessageBox.Show(
                            "Категория успешно добавлена",
                            "Успех",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Ошибка при добавлении категории: {ex.Message}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void OnDeleteCategoryButtonClick(object sender, RoutedEventArgs e)
        {
            if (CategoriesDataGrid.SelectedItem is Category selectedCategory)
            {
                var result = MessageBox.Show(
                    "Вы уверены, что хотите удалить эту категорию? Все товары в этой категории также будут удалены.",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new ApplicationDbContext())
                        {
                            var category = context.Categories
                                .Include(c => c.Products)
                                .FirstOrDefault(c => c.Id == selectedCategory.Id);

                            if (category != null)
                            {
                                context.Categories.Remove(category);
                                context.SaveChanges();
                                LoadCategories();
                                LoadProducts();
                                UpdateStatistics();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Ошибка при удалении категории: {ex.Message}",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }

        private void OnViewOrderButtonClick(object sender, RoutedEventArgs e)
        {
            var selectedOrder = OrdersDataGrid.SelectedItem as Order;
            if (selectedOrder != null)
            {
                var orderWindow = new OrderWindow(selectedOrder);
                orderWindow.Owner = this;
                orderWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Выберите заказ для просмотра", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnUpdateOrderStatusButtonClick(object sender, RoutedEventArgs e)
        {
            var selectedOrder = OrdersDataGrid.SelectedItem as Order;
            if (selectedOrder == null)
            {
                MessageBox.Show("Выберите заказ для изменения статуса", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new OrderStatusDialog(selectedOrder.Status);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var order = context.Orders
                            .Include(o => o.User)
                            .Include(o => o.OrderItems)
                                .ThenInclude(oi => oi.Product)
                            .FirstOrDefault(o => o.Id == selectedOrder.Id);

                        if (order != null)
                        {
                            order.Status = dialog.SelectedStatus;
                            context.SaveChanges();

                            // Обновляем статус в UI
                            selectedOrder.Status = dialog.SelectedStatus;
                            OrdersDataGrid.Items.Refresh();

                            MessageBox.Show("Статус заказа успешно обновлен", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении статуса заказа: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnRefreshButtonClick(object sender, RoutedEventArgs e)
        {
            LoadOrders();
            OrdersDataGrid.Items.Refresh();
            UpdateStatistics();
        }
    }
}
