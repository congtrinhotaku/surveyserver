    public class CreateMenuRequest
    {
        public string Name { get; set; } = null!;
        public string? Path { get; set; }
        public int? ParentId { get; set; }
        public string? Icon { get; set; }
        public int? OrderIndex { get; set; }
    }
