using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YMLDownloader
{
    public class Validator
    {
        private static ValidateResult s_validResult = new ValidateResult(true);

        public ValidateResult Validate(Product p) => s_validResult;
        public ValidateResult Validate(Category p) => s_validResult;
    }

    public class ValidateResult
    {
        public bool IsValid { get; }
        public string Message { get; }

        public ValidateResult(bool isValid, string message = null)
        {
            IsValid = isValid;
            Message = message;
        }
    }
}
