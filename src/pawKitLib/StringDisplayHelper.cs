namespace pawKitLib
{
    public static class StringDisplayHelper
    {
        public static string DisplayNull() => "(null)";
        public static string DisplayEmpty() => "(empty)";
        public static string DisplayValue(string? value)
        {
            if (value == null)
                return DisplayNull();
            if (value == "")
                return DisplayEmpty();
            return value;
        }
    }
}
