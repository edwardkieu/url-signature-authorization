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
        public static string ConvertNameValueCollectionToQueryString(NameValueCollection nvCollections)
        {
            if (nvCollections == null) return string.Empty;

            StringBuilder sb = new();

            foreach (string key in nvCollections.Keys)
            {
                if (string.IsNullOrWhiteSpace(key)) continue;

                string[] values = nvCollections.GetValues(key);
                if (values == null) continue;

                foreach (string value in values)
                {
                    sb.Append(sb.Length == 0 ? "?" : "&");
                    sb.AppendFormat("{0}={1}", Uri.EscapeDataString(key), HttpUtility.UrlEncode(value));
                }
            }

            return sb.ToString();
        }

        public static string RemoveQueryStrings(string url, string[] keys)
        {
            if (!url.Contains("?"))
                return url;

            var uri = new Uri(url);
            var newQueryString = HttpUtility.ParseQueryString(uri.Query);

            foreach (string key in keys)
                newQueryString.Remove(key);

            var leftPartWithoutQueryString = uri.GetLeftPart(UriPartial.Path);

            return newQueryString.Count > 0
            ? string.Format("{0}?{1}", leftPartWithoutQueryString, newQueryString)
            : leftPartWithoutQueryString;
        }

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