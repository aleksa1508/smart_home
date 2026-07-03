using Common.Models;

namespace Common.DTOs
{
    public class RuleActionDTO
    {
        public Device Device { get; set; }
        public string DeviceGroup { get; set; }
        public int? FunctionId { get; set; }

        public string FunctionName { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            string device = Device != null ? Device.Name : DeviceGroup;
            return $"{device}: {FunctionName} -> {Value}";
        }
    }
}
