using Common.Models;

namespace Common.DTOs
{
    public class CommandDTO
    {
        public Device SelectedDevice { get; set; }
        public int FunctionID { get; set; }
        public string Function { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
