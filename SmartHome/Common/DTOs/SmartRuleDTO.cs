using Common.Models;
using System.Collections.Generic;

namespace Common.DTOs
{
    public class SmartRuleDTO
    {
        public string Action { get; set; } = string.Empty;
        public List<RuleActionDTO> Actions { get; set; }
        public SmartRule SmartRule { get; set; }
    }
}
