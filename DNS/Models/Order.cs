using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace DNS.Models
{
    public enum OrderStatus
    {
        Новый,
        Собирается,
        Выполнен
    }

    public class Order : INotifyPropertyChanged
    {
        private int _id;
        private int _userId;
        private User _user;
        private DateTime _orderDate;
        private OrderStatus _status;
        private decimal _totalAmount;
        private ICollection<OrderItem> _orderItems;

        [Key]
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        [Required]
        public int UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                OnPropertyChanged();
            }
        }

        public User User
        {
            get => _user;
            set
            {
                _user = value;
                OnPropertyChanged();
            }
        }

        public DateTime OrderDate
        {
            get => _orderDate;
            set
            {
                _orderDate = value;
                OnPropertyChanged();
            }
        }

        public OrderStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged();
            }
        }

        public ICollection<OrderItem> OrderItems
        {
            get => _orderItems;
            set
            {
                _orderItems = value;
                OnPropertyChanged();
            }
        }

        public Order()
        {
            OrderItems = new List<OrderItem>();
            OrderDate = DateTime.Now;
            Status = OrderStatus.Новый;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
