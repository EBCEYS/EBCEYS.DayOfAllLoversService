namespace EBCEYS.DayOfAllLoversService.Extensions
{
    public static class IEnumerableExtensions
    {
        public static T? GetRandomElement<T>(this IEnumerable<T> enumerable)
        {
            if (!enumerable.Any())
            {
                return default;
            }
            return enumerable.ElementAt(Random.Shared.Next(0, enumerable.Count()));
        }
    }
}
