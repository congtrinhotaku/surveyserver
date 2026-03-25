    public class UpdateUserRequest
    {
        public bool? IsActive { get; set; }
        public string? Password { get; set; }
        public List<int>? RoleIds { get; set; }
    }
