using System.Text;

namespace IPMS.Business.Common.Utils
{
    public class PasswordGeneratorUtils
    {
        private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        private const string DigitChars = "1234567890";
        private const string SpecialChars = "!@#$%^&*()";

        public static string GenerateRandomPassword(int length = 12)
        {
            if (length < 4) throw new ArgumentException("Password length must be at least 4.");

            StringBuilder password = new StringBuilder();
            Random random = new Random();

            // Ensure at least one character from each required category is present
            password.Append(UppercaseChars[random.Next(UppercaseChars.Length)]);
            password.Append(LowercaseChars[random.Next(LowercaseChars.Length)]);
            password.Append(DigitChars[random.Next(DigitChars.Length)]);
            password.Append(SpecialChars[random.Next(SpecialChars.Length)]);

            // Fill the rest of the password length with random characters from all categories
            string allChars = UppercaseChars + LowercaseChars + DigitChars + SpecialChars;
            while (password.Length < length)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle the password to mix the required characters
            return ShufflePassword(password.ToString());
        }

        private static string ShufflePassword(string password)
        {
            char[] array = password.ToCharArray();
            Random rng = new Random();
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                (array[n], array[k]) = (array[k], array[n]);
            }
            return new string(array);
        }
    }
}
