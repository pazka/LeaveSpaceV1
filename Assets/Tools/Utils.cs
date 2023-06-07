namespace Tools
{
    public static class Utils
    {
        public static bool IsNullEmptyOrZero(string s)
        {
            return string.IsNullOrEmpty(s) || s == "0" || s == "null";
        }
    }
}