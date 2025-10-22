namespace FastFoodOnline.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; } // nếu có đăng nhập
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<CartItem> Items { get; set; }
    }
}
