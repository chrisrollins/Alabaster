using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alabaster
{
    internal static class RouteValidator
    {
        private const string allowedCharacters = "                                 !  $% '()*+,-. 0123456789       ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz";
        private static readonly ConcurrentDictionary<string, ValidationInfo> cachedResults = new ConcurrentDictionary<string, ValidationInfo>(Environment.ProcessorCount, 100);
        
        internal static void EnforceValidation(string route) => GetValidationInfo(route).Enforce();
        internal static bool IsValid(string route) => GetValidationInfo(route).Valid;
        internal static ValidationInfo GetValidationInfo(string route)
        {
            if(cachedResults.TryGetValue(route, out ValidationInfo info)) { return info; }

            string[] sections = route.Split('\\', '/');
            List<ValidationInfo.ValidationError> Errors = new List<ValidationInfo.ValidationError>(sections.Length);
            if (sections[0] == "") { sections[0] = "_"; }
            if (sections[sections.Length - 1] == "") { sections[sections.Length - 1] = "_"; }
            foreach (string section in sections)
            {
                ValidationInfo.ValidationError? err = validateSection(section);
                if (err != null) { Errors.Add(err.Value); }
            }

            info = Errors;
            cachedResults[route] = info;
            return info;

            ValidationInfo.ValidationError? validateSection(string s)
            {
                if (string.IsNullOrEmpty(s)) { return (ValidationInfo.ValidationError.ErrorType.REPEATED_SEPARATOR, "Repeated separator (/ or \\)"); }
                foreach(char c in s)
                {
                    if(c >= allowedCharacters.Length || allowedCharacters[c] == ' ') { return (ValidationInfo.ValidationError.ErrorType.ILLEGAL_CHARACTER, "Illegal character \'" + c + "\'"); }
                }
                return null;
            }
        }
        
        internal readonly struct ValidationInfo
        {
            public readonly bool Valid;
            public readonly ValidationError[] Errors;

            public ValidationInfo(bool valid, ValidationError[] errors)
            {
                this.Valid = valid;
                this.Errors = errors;
            }
            public void Enforce() { if (!this.Valid) { throw new FormatException("Route Validation Errors: \n" + string.Join("\n", this.Errors)); } }
            public static implicit operator ValidationInfo(ValidationError[] e) => new ValidationInfo(e.Length == 0, e);
            public static implicit operator ValidationInfo(List<ValidationError> e) => e.ToArray();
            
            public struct ValidationError
            {
                public enum ErrorType : byte { ILLEGAL_CHARACTER, REPEATED_SEPARATOR };
                public readonly ErrorType Type;
                public readonly string Message;
                public ValidationError(ErrorType type, string message)
                {
                    this.Type = type;
                    this.Message = message;
                }
                public static implicit operator ValidationError((ErrorType e, string m) args) => new ValidationError(args.e, args.m);
                public override string ToString() => this.Message;
            }
        }
    }
}
