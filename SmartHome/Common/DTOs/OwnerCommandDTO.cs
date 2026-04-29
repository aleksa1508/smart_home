using Common.Enums;

namespace Common.DTOs
{
    public class OwnerCommandDTO
    {
        public string Action { get; set; } = string.Empty;
        public User Owner { get; set; }
        public User ChangedUser { get; set; }
        public UserRole Role { get; set; }
    }
}
