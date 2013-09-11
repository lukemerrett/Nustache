using System;
using System.Text.RegularExpressions;

namespace Nustache.Core
{
    using System.Web;

    public class VariableReference : Part
    {
        private static readonly Regex _notEscapedRegex = new Regex(@"^\{\s*(.+?)\s*\}$");
        private readonly string _path;
        private readonly bool _escaped;

        public VariableReference(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            _path = path;

            var match = _notEscapedRegex.Match(path);
            _escaped = !match.Success;

            if (match.Success)
            {
                _path = match.Groups[1].Value;
            }
        }

        public string Path { get { return _path; } }

        public override void Render(RenderContext context)
        {
            object value = context.GetValue(_path);

            if (value != null)
            {
                context.Write(_escaped
                    ? Encoders.HtmlEncode(value.ToString())
                    : ReplaceLineBreaksAndEncode(value.ToString()));
                return;
            }

            if (context.CurrentOptions.PreserveUndefinedVariables && !context.PathExists(_path))
            {
                context.Write(Source());
            }
        }

        public override string Source()
        {
            return "{{" + _path + "}}";
        }

        public override string ToString()
        {
            return string.Format("VariableReference(\"{0}\")", _path);
        }

        private string ReplaceLineBreaksAndEncode(string value)
        {
            var cleansedValue = HttpUtility.HtmlEncode(value);
            return ReplaceNewLineWithBreak(cleansedValue);
        }

        private string ReplaceNewLineWithBreak(string stringToReplace)
        {
            if (string.IsNullOrEmpty(stringToReplace))
            {
                return stringToReplace;
            }

            stringToReplace = stringToReplace.Trim(Environment.NewLine.ToCharArray());
            stringToReplace = stringToReplace.Replace(Environment.NewLine, "<br />");
            stringToReplace = stringToReplace.Replace("\n", "<br />");
            stringToReplace = stringToReplace.Replace("&#13;&#10;", "<br />"); // Cater for AntiXss encoded line breaks
            return stringToReplace;
        }
    }
}