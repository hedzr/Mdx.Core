using System.Diagnostics.Contracts;

namespace HzNS.Ext.Demo
{
    public static class StringEx
    {
        public static string EllipseStart(string s, int max = 30)
        {
            if (s.Length > max)
            {
                return "..." + s.Substring(s.Length - max);
            }

            return s;
        }
        
        [Pure]
        // [PublicAPI]
        // ReSharper disable once BuiltInTypeReferenceStyle
        public static string Repeat(char ch, int repeatCount)
        {
            return new string(ch, repeatCount);
        }


        public static bool ToBool(object s, bool defaultValue = false)
        {
            return s switch
            {
                string s1 => ToBool(s1, defaultValue),
                null => defaultValue,
#pragma warning disable CS8604
                _ => ToBool(s.ToString(), defaultValue)
#pragma warning restore CS8604
            };
        }

        public static bool ToBool(string s, bool defaultValue = false)
        {
            return s.ToLower() switch
            {
                "1" => true,
                "yes" => true,
                "y" => true,
                "true" => true,
                "t" => true,
                "on" => true,
                "是" => true,
                "真" => true,
                "0" => false,
                "no" => false,
                "n" => false,
                "false" => false,
                "f" => false,
                "off" => false,
                "否" => false,
                "假" => false,
                _ => defaultValue
            };
        }
    }
}