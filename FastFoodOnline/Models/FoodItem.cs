using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FastFoodOnline.Enums;

namespace FastFoodOnline.Models
{
    public class FoodItem
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Tên món ăn")]
        public string Name { get; set; }

        [StringLength(250)]
        public string Description { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Url]
        public string ImageUrl { get; set; }
        public ItemStatus Status { get; set; } = ItemStatus.Available;


        [Required] // ← Có thể gây lỗi nếu không đúng
        public int CategoryId { get; set; }
        [ValidateNever]
        public Category Category { get; set; }
        [ValidateNever]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        [ValidateNever]
        public virtual ICollection<ComboItem> ComboItems { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}