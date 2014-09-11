using System.Globalization;
namespace Cyber.Util
{
    internal static class TextHandling
    {
        private static NumberFormatInfo nfi = new NumberFormatInfo();

        internal static void replaceDecimal()
        {
            nfi.NumberDecimalSeparator = ".";
        }

        internal static string GetString(double k)
        {
            return k.ToString(nfi);
        }

        internal static int Parse(string a)
        {
            int w = 0, i = 0, length = a.Length, k;

            if (length == 0)
                return 0;

            do
            {
                k = a[i++];
                if (k < 48 || k > 59)
                    return 0;
                w = 10 * w + k - 48;
            }
            while (i < length);

            return w;
        }
    }
}
