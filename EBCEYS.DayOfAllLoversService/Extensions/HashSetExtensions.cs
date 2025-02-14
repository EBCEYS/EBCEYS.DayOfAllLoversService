namespace EBCEYS.DayOfAllLoversService.Extensions
{
    public static class HashSetExtensions
    {
        public static T? GetRandomElement<T>(this HashSet<T> set)
        {
            if (set.Count == 0)
            {
                return default;
            }
            return set.ElementAt(Random.Shared.Next(0, set.Count));
        }
    }
}
