using System;
using System.Linq;

namespace BehaveBot.Classes
{
    public static class StringExtensions
    {

        public static string CenterString(this string value)
        {
            return String.Format("{0," + ((Console.WindowWidth / 2) + ((value).Length / 2)) + "}", value);
        }

        public static string RemoveSpaces(this string value)
        {

            value = value.Trim(' ');

            return value;
        }

        public static string RemoveString(string text, string removingText)
        {
            text = text.Substring(removingText.Length).Trim();

            return text;
        }
    }
}
