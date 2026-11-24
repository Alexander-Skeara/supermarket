namespace ObuvShop.Models
{
    public class LookupItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public override string ToString() => Name;
    }
}
