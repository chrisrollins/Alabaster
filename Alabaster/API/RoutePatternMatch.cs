using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alabaster
{
    // * match all characters (eg. /api*)
    // : parameter (eg. /api/:foo or /api/:foo/) only alphanumeric, underscore, and hyphen allowed
    // 
    public struct RoutePatternMatch
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
            return (Request req) =>
            {
                RoutePatternMatchResult result = This.Compare(req.cw.Route);
                req.Parameters = result.Parameters;
                return (result.Match) ? cb.Callback(req) : new PassThrough();
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
                if(paramStr[0] != ':') { return false; }
                for(int i = 1; i < paramStr.Length; i++)
                {
                    if(parameterAllowedCharacters[paramStr[i]] == ' ') { return false; }
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
                        string name = this.Specifier.Substring(i, (pNameEnd - i));
                        string value = reqUrl.Substring(inputStrPosition, (pValueEnd - inputStrPosition));
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

    internal struct RoutePatternMatchResult
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
