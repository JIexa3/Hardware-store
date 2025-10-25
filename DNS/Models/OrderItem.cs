using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DNS.Models
{
    public class OrderItem : INotifyPropertyChanged
    {
        private int _id;
        private int _orderId;
        private Order _order;
        private int _productId;
        private Product _product;
        private int _quantity;
        private decimal _price;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public int OrderId
        {
            get => _orderId;
            set
            {
                _orderId = value;
                OnPropertyChanged();
            }
        }

        public Order Order
        {
            get => _order;
            set
            {
                _order = value;
                OnPropertyChanged();
            }
        }

        public int ProductId
        {
            get => _productId;
            set
            {
                _productId = value;
                OnPropertyChanged();
            }
        }

        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged();
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public decimal TotalPrice => Price * Quantity;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
