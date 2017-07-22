using System;

namespace BnTxx.Formats
{
    class InvalidSignatureException : Exception
    {
        private const string ExMsg = "Invalid signature! Expected {0}, found {1}!";

        public InvalidSignatureException() : base() { }

        public InvalidSignatureException(string Expected, string Found) : base(string.Format(ExMsg, Expected, Found)) { }
    }
}
