using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ClassDocumentGenerator.Lib
{
    public static class Extensions
    {
        public static bool EqualTag(this XmlElementStartTagSyntax startTagSyntax, CommentTag commentTag)
        {
            return startTagSyntax.Name.ToString().ToLower() == commentTag.ToString().ToLower();
        }

        public static bool HasStartTagContent(this XmlElementSyntax elementSyntax)
        {
            var commentTag = elementSyntax.StartTag.Name.ToString().ToLower();

            var commentTags = Enum.GetValues<CommentTag>().Select(tag => tag.ToString().ToLower());

            return commentTags.Contains(commentTag);
        }

        public static CommentTag GetStartCommentTag(this XmlElementStartTagSyntax startTagSyntax)
        {
            var commentTag = startTagSyntax.Name.ToString().ToLower();

            if (commentTag == CommentTag.Summary.ToString().ToLower()) return CommentTag.Summary;
            if (commentTag == CommentTag.Remarks.ToString().ToLower()) return CommentTag.Remarks;
            if (commentTag == CommentTag.Code.ToString().ToLower()) return CommentTag.Code;
            if (commentTag == CommentTag.Param.ToString().ToLower()) return CommentTag.Param;
            if (commentTag == CommentTag.Returns.ToString().ToLower()) return CommentTag.Returns;

            throw new InvalidOperationException();
        }

        public static TagContent GetTagContent(this XmlElementSyntax elementSyntax)
        {
            var startCommentTag = elementSyntax.StartTag.GetStartCommentTag();

            var values =
                elementSyntax.Content.ToString()
                    .Replace("///", string.Empty)
                    .Split("\r\n")
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s));

            var xElement = XElement.Parse(elementSyntax.ToString());
            var parameterName = xElement.Attribute("name")?.Value;

            var join = string.Join("\r\n", values);
            return new TagContent(startCommentTag, join, parameterName);
        }

        public static string ConvertFileNameToUsableString(this string old)
        {
            const string restrictChars = "|\\\\?*<\":>/";
            var regExpr = $"[{restrictChars}]+";

            // 파일명으로 사용 불가능한 특수문자 제거
            var tmpStr = Regex.Replace(old, regExpr, "_");

            // 공백문자 "_"로 치환
            return tmpStr.Replace("[ ]", "_");
        }
    }
}