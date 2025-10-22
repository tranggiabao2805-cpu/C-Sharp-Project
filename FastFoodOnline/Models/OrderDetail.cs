using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFoodOnline.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }   // 🔑 Khóa chính tự tăng

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int? FoodItemId { get; set; }
        public FoodItem FoodItem { get; set; }

        public int? ComboId { get; set; }
        public Combo Combo { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [StringLength(250)]
        public string? Note { get; set; }

        public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;

        [NotMapped]
        public decimal CalculatedTotal => Quantity * UnitPrice;
    }
}