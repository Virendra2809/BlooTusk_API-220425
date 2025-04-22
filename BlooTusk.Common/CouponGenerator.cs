using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Common
{
    public class CouponGenerator
    { 
   
        private static readonly Random random = new Random();

        public static string GenerateCouponCode()
        {
            // Determine the year character based on the current year
            char yearChar = (char)('A' + (DateTime.Now.Year - 2023));

            // Generate the 4 random characters [A-Z & 0-9]
            string randomChars = GenerateRandomChars(4, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");

            // Combine the year character and random characters
            return yearChar + randomChars;
        }

        public static string GenerateCouponSeries()
        {
            // Generate 3 random characters [A-Z & 0-9]
            return GenerateRandomChars(3, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
        }

        private static string GenerateRandomChars(int length, string validChars)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(validChars.Length);
                sb.Append(validChars[index]);
            }
            return sb.ToString();
        }


        public static string  CouponCode()
        {
            string CouponCode = "";
            string couponCode = GenerateCouponCode();
            string couponSeries = GenerateCouponSeries();
         return couponCode = couponCode + couponSeries;
        }
        
    }

}

