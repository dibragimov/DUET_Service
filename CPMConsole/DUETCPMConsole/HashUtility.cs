using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUETCPMConsole
{
    public class HashUtility
    {
        private static string secretKey = "qwerty";
        public static string CalculateMD5(long sessionId, int clientAccountId, int ctrgAccountId, int ctrgClientAccountId, int contractBindId, string externalDocDate, string externalDocNumber, decimal feeAmount, int functionType, string paymentDetails, decimal transactAmount)
        {
            // step 1, calculate MD5 hash from input
            
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            string resultingString = sessionId.ToString() + clientAccountId.ToString() + transactAmount.ToString() + secretKey;
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(resultingString);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2")); //UPPER case hexadecimal conversion
            }
            return sb.ToString();
        }

        public static string CalculateMD5(string date)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            string resultingString = date + secretKey;
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(resultingString);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2")); //UPPER case hexadecimal conversion
            }
            return sb.ToString();
        }
    }
}
