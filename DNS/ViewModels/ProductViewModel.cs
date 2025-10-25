using System.ComponentModel;
using System.Runtime.CompilerServices;
using DNS.Models;

namespace DNS.ViewModels
{
    public class ProductViewModel : INotifyPropertyChanged
    {
        private readonly Product _product;

        public ProductViewModel(Product product)
        {
            _product = product;
        }

        public int Id => _product.Id;

        public string Name
        {
            get => _product.Name;
            set
            {
                if (_product.Name != value)
                {
                    _product.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => _product.Description;
            set
            {
                if (_product.Description != value)
                {
                    _product.Description = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal Price
        {
            get => _product.Price;
            set
            {
                if (_product.Price != value)
                {
                    _product.Price = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ImageUrl
        {
            get => _product.ImageUrl;
            set
            {
                if (_product.ImageUrl != value)
                {
                    _product.ImageUrl = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CategoryId
        {
            get => _product.CategoryId;
            set
            {
                if (_product.CategoryId != value)
                {
                    _product.CategoryId = value;
                    OnPropertyChanged();
                }
            }
        }

        public int StockQuantity
        {
            get => _product.StockQuantity;
            set
            {
                if (_product.StockQuantity != value)
                {
                    _product.StockQuantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public Category Category
        {
            get => _product.Category;
            set
            {
                if (_product.Category != value)
                {
                    _product.Category = value;
                    OnPropertyChanged();
                }
            }
        }

        public Product ToModel() => _product;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
