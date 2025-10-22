using System.ComponentModel.DataAnnotations;

namespace FastFoodOnline.Models
{
    public class ComboItem
    {
        public int ComboId { get; set; }
        public Combo Combo { get; set; }

        public int FoodItemId { get; set; }
        public FoodItem FoodItem { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; } = 1;

    }
}
