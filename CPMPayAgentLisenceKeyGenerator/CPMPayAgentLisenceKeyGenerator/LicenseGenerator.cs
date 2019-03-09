using System;
using System.Security.Cryptography;
using System.Text;

namespace LicenseManager
{
    class LicenseGenerator
    {
        public static DateTime UnlimitedLicenceValidUntil = new DateTime(3000, 1, 1);

        public static string GenerateLicence(string hwInfo, string appCode, DateTime validUntil)
        {
            var key = ComputeHash(hwInfo + appCode);
            var IV = ComputeHash(hwInfo);
            return Encode(key, IV, validUntil);
        }

        private static byte[] ComputeHash(string message)
        {
            var baseBytes = Encoding.ASCII.GetBytes(message);
            var md5 = MD5.Create();
            return md5.ComputeHash(baseBytes);
        }

        private static string ConvertToString(byte[] message)
        {
            return Convert.ToBase64String(message);
        }

        private static string Encode(byte[] key, byte[] IV,  DateTime date)
        {
            var aes = TripleDES.Create();
            if (aes == null)
                return string.Empty;
            var encriptor = aes.CreateEncryptor(key, IV);
            var ticks = Encoding.ASCII.GetBytes(date.Ticks.ToString());

            var message = encriptor.TransformFinalBlock(ticks, 0, ticks.Length);
            return ConvertToString(message);
        }
    }
}
