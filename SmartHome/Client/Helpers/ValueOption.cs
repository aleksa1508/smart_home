namespace Client.Helpers
{
    public class ValueOption
    {
        public int? FunctionId { get; set; }
        public string Name { get; set; } = string.Empty;

        public override string ToString() => Name;
    }
}
