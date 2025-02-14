namespace EBCEYS.DayOfAllLoversService.Extensions
{
    public static class StringExtensions
    {
        public static bool IsValidPath(this string path)
        {
            return Path.IsPathRooted(path);
        }
    }
}
