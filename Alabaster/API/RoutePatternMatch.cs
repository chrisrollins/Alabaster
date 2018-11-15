using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alabaster
{
    using RouteCallback_A = Func<Request, Response>;
    using RouteCallback_B = Action<Request>;
    using RouteCallback_C = Func<Response>;
    using RouteCallback_D = Action;

    // * match all characters (eg. /api*)
    // : parameter (eg. /api/:foo or /api/:foo/) only alphanumeric, underscore, and hyphen allowed
    // 
    public readonly struct RoutePatternMatch
    {
        public readonly string Specifier;
        public RoutePatternMatch(string specifier)
        {
            Validate(specifier);
            this.Specifier = Normalize(specifier);
        }
        public static explicit operator RoutePatternMatch(string specifier) => new RoutePatternMatch(specifier);

        const string patternSpecifierChars = ":*";
        const string parameterAllowedCharacters = "                                             -  0123456789       ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz";

        internal RouteCallback_A CreateCallback(RouteCallback cb)
        {
            RoutePatternMatch This = this;
            RouteCallback_A callback = cb.Callback;
            return (Request req) =>
            {
                RoutePatternMatchResult result = This.Compare(req.cw.Route);
                req.Parameters = new NameValueCollection(result.Parameters.Length);
                foreach((string name, string value) in result.Parameters)
                {
                    req.Parameters.Add(name, value);
                }
                return (result.Match) ? callback(req) : PassThrough.Default;
            };
        }

        private static void Validate(string specifier)
        {
            RouteValidator.EnforceValidation(Util.ReplaceMultiple(specifier, patternSpecifierChars, '_'));
            string[] sections = specifier.Split('/', '\\');
            foreach(string section in sections)
            {               
                if(CountSpecifierChars(section) > 1 || (!ValidateParam(section) && !ValidateWild(section))) { throw new ArgumentException("Invalid URL Pattern specifier."); }
            }

            bool ValidateParam(string paramStr)
            {
                if(string.IsNullOrEmpty(paramStr) || paramStr[0] != ':') { return false; }
                for(int i = 1; i < paramStr.Length; i++)
                {
                    char current = paramStr[i];
                    if (current >= parameterAllowedCharacters.Length || current < 0 || parameterAllowedCharacters[current] == ' ') { return false; }
                }
                return true;
            }

            bool ValidateWild(string wildCardStr)
            {
                for(int i = 1; i < wildCardStr.Length; i++)
                {
                    if(wildCardStr[i - 1] == '*' && wildCardStr[i] == '*') { return false; }
                }
                return true;
            }

            int CountSpecifierChars(string s)
            {
                int count = 0;
                foreach(char c in patternSpecifierChars)
                {
                    count += (s.Contains(c)) ? 1 : 0;
                }
                return count;
            }
        }

        private RoutePatternMatchResult Compare(string reqUrl)
        {
            reqUrl = Normalize(reqUrl);         
            List<(string, string)> par = new List<(string, string)>(20);
            RoutePatternMatch This = this;
            int inputStrPosition = 0;
            for (int i = 0; i < this.Specifier.Length; i++)
            {
                char current = this.Specifier[i];
                char next = this.Specifier[i + 1];
                switch(current)
                {
                    case '*':
                        if(i == this.Specifier.Length - 1) { return Result(true); }
                        inputStrPosition = reqUrl.IndexOf(next, inputStrPosition);
                        int nextWild = reqUrl.IndexOf(next, '*');
                        if(inputStrPosition == -1 || nextWild < inputStrPosition) { return Result(false); }
                        break;
                    case ':':
                        int pValueEnd = reqUrl.IndexOf('/', inputStrPosition);
                        int pNameEnd = this.Specifier.IndexOf('/', i);
                        if(pNameEnd == -1) { pNameEnd = this.Specifier.Length - 1; }
                        if(pValueEnd == -1) { pValueEnd = reqUrl.Length - 1; }
                        string name = this.Specifier.Substring(i + 1, (pNameEnd - i));
                        string value = reqUrl.Substring(inputStrPosition, (pValueEnd - inputStrPosition + 1));
                        par.Add((name, value));
                        inputStrPosition = pValueEnd + 1;
                        if(inputStrPosition >= reqUrl.Length) { return Result(true); }
                        break;
                    default:
                        if(inputStrPosition == reqUrl.Length - 1 && reqUrl[inputStrPosition] == current) { return Result(true); } 
                        if(inputStrPosition >= reqUrl.Length || reqUrl[inputStrPosition] != current) { return Result(false); }
                        inputStrPosition++;
                        break;
                }
            }

            return Result(false);
            RoutePatternMatchResult Result(bool match) => new RoutePatternMatchResult(reqUrl, This, match, par.ToArray());
        }

        private static string Normalize(string s) => s.Replace('\\', '/').Trim('/');
    }

    internal readonly ref struct RoutePatternMatchResult
    {
        public readonly string InputString;
        public readonly RoutePatternMatch Pattern;
        public readonly bool Match;
        public readonly (string Name, string Value)[] Parameters;
        internal RoutePatternMatchResult(string input, RoutePatternMatch pattern, bool match, (string, string)[] par)
        {
            this.InputString = input;
            this.Pattern = pattern;
            this.Match = match;
            this.Parameters = par;
        }
    }
}
