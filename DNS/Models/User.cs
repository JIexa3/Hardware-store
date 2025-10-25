using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace DNS.Models
{
    public class User : INotifyPropertyChanged
    {
        private int _id;
        private string _username;
        private string _password;
        private string _email;
        private bool _isAdmin;
        private bool _isEmailVerified;
        private DateTime _registerDate;

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
        [StringLength(50)]
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        [Required]
        [StringLength(100)]
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public bool IsAdmin
        {
            get => _isAdmin;
            set
            {
                _isAdmin = value;
                OnPropertyChanged();
            }
        }

        public bool IsEmailVerified
        {
            get => _isEmailVerified;
            set
            {
                _isEmailVerified = value;
                OnPropertyChanged();
            }
        }

        public DateTime RegisterDate
        {
            get => _registerDate;
            set
            {
                _registerDate = value;
                OnPropertyChanged();
            }
        }

        public User()
        {
            RegisterDate = DateTime.Now;
            IsAdmin = false;
            IsEmailVerified = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
