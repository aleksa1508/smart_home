namespace Common.Models
{
    public class RuleAction
    {
        public int RuleId { get; set; }
        public int? DeviceId { get; set; }
        public string DeviceGroup { get; set; }
        public int? FunctionId { get; set; }
        public string FunctionName { get; set; }
        public string Value { get; set; }
    }
}
