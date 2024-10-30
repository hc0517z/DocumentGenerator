namespace ClassDocumentGenerator.Lib
{
    public class TagContent
    {
        public TagContent(CommentTag tag, string content, string parameterName)
        {
            Tag = tag;
            Content = content;
            ParameterName = parameterName;
        }

        public CommentTag Tag { get; }

        public string Content { get; }
        
        public string ParameterName { get; }

        public override string ToString()
        {
            return $"<< {Tag} >>\r\n{Content}";
        }
    }
}