using System.Collections.Generic;

namespace ClassDocumentGenerator.Lib.DocumentWriter
{
    public class DocumentWriterFactory
    {
        public static IDocumentWriter CreateDocumentWriter(Dictionary<string, string> parameters, string path)
        {
            if (parameters.ContainsKey("-word")) return new WordDocumentWriter(path);

            return new HwpDocumentWriter(path);
        }
    }
}