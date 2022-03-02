using System;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Common.Lib.Helpers
{
    public static class StringHelper
    {
        public static string GenerateSignature(string message, string key)
        {
            using (var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(key)))
            {
                byte[] hashMessage = hmac.ComputeHash(Encoding.ASCII.GetBytes(message));
                return string.Concat(hashMessage.Select(b => b.ToString("x2")));
            }
        }
    }
}