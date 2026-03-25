        public class CreateUserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public List<int>? RoleIds { get; set; }
    }