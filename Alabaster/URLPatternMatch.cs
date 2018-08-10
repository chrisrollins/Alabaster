using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alabaster
{
    public struct URLPatternMatch
    {
        public string PatternSpecifier;
        public URLPatternMatch(string specifier) => this.PatternSpecifier = specifier;
        public static explicit operator URLPatternMatch(string specifier) => new URLPatternMatch(specifier);


        // * match all characters (eg. /api*)
        // /[]/ encloses parameter name (eg. /api/[foo] or /api/[foo]/)
        // 

    }
}
