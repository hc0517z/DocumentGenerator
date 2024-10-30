using System;
using System.Collections.Generic;

namespace ClassDocumentGenerator.Lib.DocumentWriter
{
    public interface IDocumentWriter : IDisposable
    {
        public bool ExistsDocumentFile(string fileName);
        public string CreateDocumentFile(string fileName);
        public void WriteClassTable(ClassDetails classDetails);
        public void WriteFunctionDetails(FunctionDetails details);
        public bool Save();
        public bool Merge(Dictionary<string, List<string>> groups);
    }
}