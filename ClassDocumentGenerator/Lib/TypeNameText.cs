﻿using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ClassDocumentGenerator.Lib
{
    public class TypeNameText
    {
        public string Identifier { get; set; }

        public string TypeArguments { get; set; }

        public static TypeNameText From(SimpleNameSyntax syntax)
        {
            var identifier = syntax.Identifier.Text;
            var typeArgs = string.Empty;
            if (syntax is GenericNameSyntax genericName && genericName.TypeArgumentList != null)
            {
                var count = genericName.TypeArgumentList.Arguments.Count;
                identifier = $"\"{identifier}`{count}\"";
                typeArgs = "<" + string.Join(",", genericName.TypeArgumentList.Arguments) + ">";
            }
            else if (identifier.StartsWith("@"))
            {
                identifier = $"\"{identifier}\"";
            }

            return new TypeNameText
            {
                Identifier = identifier,
                TypeArguments = typeArgs
            };
        }

        public static TypeNameText From(GenericNameSyntax syntax)
        {
            var paramCount = syntax.TypeArgumentList.Arguments.Count;
            var parameters = new string[paramCount];
            if (paramCount > 1)
                for (var i = 0; i < paramCount; i++)
                    parameters[i] = $"T{i + 1}";
            else
                parameters[0] = "T";
            return new TypeNameText
            {
                Identifier = $"\"{syntax.Identifier.Text}`{paramCount}\"",
                TypeArguments = "<" + string.Join(",", parameters) + ">"
            };
        }

        public static TypeNameText From(BaseTypeDeclarationSyntax syntax)
        {
            var identifier = syntax.Identifier.Text;
            var typeArgs = string.Empty;
            if (syntax is TypeDeclarationSyntax typeDeclaration && typeDeclaration.TypeParameterList != null)
            {
                typeArgs = $"<{string.Join(",", typeDeclaration.TypeParameterList.Parameters)}>";
                identifier = $"{identifier}{typeArgs}";
            }
            else if (identifier.StartsWith("@"))
            {
                identifier = $"\"{identifier}\"";
            }

            return new TypeNameText
            {
                Identifier = identifier,
                TypeArguments = typeArgs
            };
        }
    }
}