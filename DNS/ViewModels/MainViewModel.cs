using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Diagnostics;
using DNS.Models;
using DNS.Data;
using Microsoft.EntityFrameworkCore;

namespace DNS.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationDbContext _context;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<Product> _products;
        private ObservableCollection<Product> _allProducts;
        private string _searchQuery;

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged();
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }

        public MainViewModel()
        {
            Debug.WriteLine("Инициализация MainViewModel");
            _context = new ApplicationDbContext();
            LoadData();
            Debug.WriteLine($"Категорий: {Categories?.Count ?? 0}");
            Debug.WriteLine($"Товаров: {Products?.Count ?? 0}");
        }

        private void LoadData()
        {
            Debug.WriteLine("Начало LoadData");
            
            // Загружаем категории
            var categoriesList = _context.Categories.ToList();
            categoriesList.Insert(0, new Category { Id = 0, Name = "Все товары", Description = "Все доступные товары" });
            Categories = new ObservableCollection<Category>(categoriesList);

            // Загружаем товары
            _allProducts = new ObservableCollection<Product>(_context.Products.ToList());
            Products = new ObservableCollection<Product>(_allProducts);

            Debug.WriteLine($"Конец LoadData. Категорий: {Categories.Count}, Товаров: {Products.Count}");
        }

        private void FilterProducts()
        {
            var query = _allProducts.AsEnumerable();

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(p => 
                    p.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            }

            Products = new ObservableCollection<Product>(query);
        }

        public void FilterByCategory(Category category)
        {
            if (category == null || category.Id == 0)
            {
                Products = new ObservableCollection<Product>(_allProducts);
                return;
            }

            var filteredProducts = _allProducts.Where(p => p.CategoryId == category.Id).ToList();
            Products = new ObservableCollection<Product>(filteredProducts);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
