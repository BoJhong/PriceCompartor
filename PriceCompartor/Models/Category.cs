using System.ComponentModel;

namespace PriceCompartor.Models
{
    public class Category
    {
        public int Id { get; set; }

        [DisplayName("Category")]
        public required string Name { get; set; }
    }
}
