using System.ComponentModel.DataAnnotations;
using FastFoodOnline.Models;

namespace FastFoodOnline.ViewModels.Admin
{
    public class ComboCreateViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public Combo.ComboStatus Status { get; set; } = Combo.ComboStatus.Active;

        public List<ComboFoodItem> Items { get; set; } = new();
    }

    public class ComboFoodItem
    {
        public int FoodItemId { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
        public int Quantity { get; set; } = 1;
    }
}