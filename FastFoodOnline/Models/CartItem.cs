namespace FastFoodOnline.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }
        public Cart Cart { get; set; }

        public int? FoodItemId { get; set; }
        public FoodItem? FoodItem { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; } // lưu giá tại thời điểm thêm
        public int? ComboId { get; set; } // ✅ Thêm dòng này
        public Combo? Combo { get; set; }
    }
}
