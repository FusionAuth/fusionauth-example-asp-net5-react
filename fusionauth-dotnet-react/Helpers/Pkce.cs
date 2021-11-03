using System;
using System.Security.Cryptography;
using System.Text;

namespace fusionauth_dotnet_react.Helpers
{
    public static class Pkce
    {
        public static string GenerateCodeVerifier()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            //var rawCodeVerifier = "5xc_alID61sfw92WCYO4fMQo6CLHvJCk_8d3X8IJCQk"; // Base64UrlTextEncoder.Encode(bytes);
            var codeVerifier = bytes.Base64UrlEncode();
            return codeVerifier;
        }

        public static string GenerateChallenge(string codeVerifier)
        {
            var sha256 = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));

            //StringBuilder builder = new StringBuilder();
            //for (int i = 0; i < sha256.Length; i++)
            //{
            //    builder.Append(sha256[i].ToString("x2"));
            //}
            //var rawResult = builder.ToString();
            var result = sha256.Base64UrlEncode();
            return result;
        }

        public static string GenerateNonce()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            var Nonce = bytes.Base64UrlEncode();
            return Nonce;
        }

        private static string Base64UrlEncode(this byte[] sha256)
        {
            return Convert.ToBase64String(sha256)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", string.Empty);
        }
    }
}
