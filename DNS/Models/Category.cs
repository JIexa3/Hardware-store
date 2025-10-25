using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DNS.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
