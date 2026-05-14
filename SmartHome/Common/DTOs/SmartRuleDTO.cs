using Common.Models;

namespace Common.DTOs
{
    public class SmartRuleDTO
    {
        public string Action { get; set; } = string.Empty;
        public SmartRule SmartRule { get; set; }
    }
}
