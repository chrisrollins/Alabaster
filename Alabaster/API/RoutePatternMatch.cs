using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alabaster
{
    public struct RoutePatternMatch
    {
        public readonly string PatternSpecifier;
        public RoutePatternMatch(string specifier) => this.PatternSpecifier = specifier;
        public static explicit operator RoutePatternMatch(string specifier) => new RoutePatternMatch(specifier);

        const string patternSpecifierChars = "[]*";
        const string parameterAllowedCharacters = "                                             -  0123456789       ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz";

        private void Parse(string specifier)
        {
            string ValidationString = Util.ReplaceMultiple(specifier, patternSpecifierChars, '_');
            RouteValidator.EnforceValidation(ValidationString);
            string[] sections = specifier.Split('/', '\\');
        }

        // * match all characters (eg. /api*)
        // /[]/ encloses parameter name (eg. /api/[foo] or /api/[foo]/) only alphanumeric, underscore, and hyphen allowed
        // 

    }
}
