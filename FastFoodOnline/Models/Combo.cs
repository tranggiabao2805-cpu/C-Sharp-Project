namespace FastFoodOnline.Models
{
    public class Combo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public enum ComboStatus { Active, Inactive }

        public ComboStatus Status { get; set; } = ComboStatus.Active;


        public ICollection<ComboItem> ComboItems { get; set; }
    }
}
