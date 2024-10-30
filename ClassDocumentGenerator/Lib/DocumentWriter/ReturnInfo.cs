namespace ClassDocumentGenerator.Lib.DocumentWriter
{
    public class ReturnInfo
    {
        public string Description { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public override string ToString()
        {
            return IsEmpty ? "void" : $"{Type}: {Description}";
        }
        
        public bool IsEmpty => string.IsNullOrEmpty(Description) && string.IsNullOrEmpty(Type);
    }
}