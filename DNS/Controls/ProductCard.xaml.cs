using System.Windows;
using System.Windows.Controls;
using DNS.Models;

namespace DNS.Controls
{
    public partial class ProductCard : UserControl
    {
        public static readonly DependencyProperty ProductProperty =
            DependencyProperty.Register("Product", typeof(Product), typeof(ProductCard), 
                new PropertyMetadata(null, OnProductChanged));

        public Product Product
        {
            get { return (Product)GetValue(ProductProperty); }
            set { SetValue(ProductProperty, value); }
        }

        public ProductCard()
        {
            InitializeComponent();
        }

        private static void OnProductChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ProductCard;
            var product = e.NewValue as Product;

            if (control != null && product != null)
            {
                control.UpdateProductInfo();
            }
        }

        private void UpdateProductInfo()
        {
            ProductName.Text = Product.Name;
            ProductDescription.Text = Product.Description;
            ProductPrice.Text = $"{Product.Price:C}";
            
            if (!string.IsNullOrEmpty(Product.ImageUrl))
            {
                try
                {
                    ProductImage.Source = new System.Windows.Media.Imaging.BitmapImage(
                        new System.Uri(Product.ImageUrl));
                }
                catch
                {
                    // Если изображение не удалось загрузить, можно установить изображение по умолчанию
                }
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Реализовать добавление в корзину
            MessageBox.Show($"Товар {Product.Name} добавлен в корзину");
        }
    }
}
