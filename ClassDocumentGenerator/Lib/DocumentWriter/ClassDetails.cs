using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassDocumentGenerator.Lib.DocumentWriter
{
    public class ClassDetails
    {
        private string _className = string.Empty;
        private List<ClassTableMember> _classTableMembers = new();
        private string _description = string.Empty;
        private string _groupName = string.Empty;

        public bool IsEmpty => string.IsNullOrEmpty(GroupName) &
                               string.IsNullOrEmpty(ClassName) &
                               string.IsNullOrEmpty(Description) &
                               (ClassTableMembers.Count == 0);

        public string GroupName
        {
            get => _groupName;
            set => _groupName = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string ClassName
        {
            get => _className;
            set => _className = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Description
        {
            get => _description;
            set => _description = value ?? throw new ArgumentNullException(nameof(value));
        }

        public List<ClassTableMember> ClassTableMembers
        {
            get => _classTableMembers;
            set => _classTableMembers = value ?? throw new ArgumentNullException(nameof(value));
        }

        public void Reset()
        {
            GroupName = string.Empty;
            ClassName = string.Empty;
            Description = string.Empty;
            ClassTableMembers.Clear();
        }

        public override string ToString()
        {
            var str = string.Join("\r\n", ClassTableMembers.Select(member => member.ToString()));
            return $@"
    {nameof(GroupName)}: {GroupName}
    {nameof(ClassName)}: {ClassName}
    {nameof(Description)}: {Description}
    {nameof(ClassTableMembers)}
        {nameof(ClassTableMember.MemberType)}|{nameof(ClassTableMember.TypeAndReturnType)}|{nameof(ClassTableMember.Name)}|{nameof(ClassTableMember.FunctionDescription)}
{str}
";
        }
    }
}