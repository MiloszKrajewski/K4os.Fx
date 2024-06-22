namespace K4os.Fx;

public static class CollectionExtensions
{
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? subject) => subject ?? [];

    public static T[] EmptyIfNull<T>(this T[]? subject) => subject ?? [];
}
