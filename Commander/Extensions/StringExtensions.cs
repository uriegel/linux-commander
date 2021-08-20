static class StringExtensions
{
    public static long ParseLong(this string text)
    {
        long.TryParse(text, out var val);
        return val;
    }
}