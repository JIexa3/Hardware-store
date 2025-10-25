using System;
using System.Windows;
using DNS.Data;
using DNS.Models;

namespace DNS.Views
{
    public partial class AddEditCategoryWindow : Window
    {
        private readonly ApplicationDbContext _context;
        private readonly Category? _category;

        public AddEditCategoryWindow(Category? category = null)
        {
            InitializeComponent();
            _context = new ApplicationDbContext();
            _category = category;

            if (_category != null)
            {
                LoadCategoryData();
                Title = "Редактирование категории";
            }
            else
            {
                Title = "Новая категория";
            }
        }

        private void LoadCategoryData()
        {
            if (_category == null) return;

            NameTextBox.Text = _category.Name;
            DescriptionTextBox.Text = _category.Description;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название категории", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_category == null)
                {
                    var newCategory = new Category
                    {
                        Name = NameTextBox.Text.Trim(),
                        Description = DescriptionTextBox.Text?.Trim()
                    };
                    _context.Categories.Add(newCategory);
                }
                else
                {
                    _category.Name = NameTextBox.Text.Trim();
                    _category.Description = DescriptionTextBox.Text?.Trim();
                    _context.Categories.Update(_category);
                }

                _context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении категории: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
