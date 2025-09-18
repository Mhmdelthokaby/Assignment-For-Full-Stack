namespace Assignment_For_Full_Stack.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime? LastLoginTime { get; set; }
    }
}
