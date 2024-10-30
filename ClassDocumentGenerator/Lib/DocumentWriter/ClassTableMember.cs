using System;

namespace ClassDocumentGenerator.Lib.DocumentWriter
{
    public class ClassTableMember
    {
        private string _functionDescription = string.Empty;

        private string _name = string.Empty;
        private string _typeAndReturnType = string.Empty;

        public ClassTableMemberType MemberType { get; set; }

        public string TypeAndReturnType
        {
            get => _typeAndReturnType;
            set => _typeAndReturnType = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Name
        {
            get => _name;
            set => _name = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string FunctionDescription
        {
            get => _functionDescription;
            set => _functionDescription = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override string ToString()
        {
            return $@"        {MemberType}|{TypeAndReturnType}|{Name}|{FunctionDescription}";
        }
    }
}