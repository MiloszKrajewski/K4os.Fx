namespace K4os.Fx;

public static class ObjectExtensions
{
    public static TimeSpan Times(this TimeSpan timeSpan, double factor) =>
        TimeSpan.FromTicks((long)(timeSpan.Ticks * factor));

    public static T NotLessThan<T>(this T value, T min) where T: IComparable<T> =>
        Comparer<T>.Default.Compare(value, min) < 0 ? min : value;

    public static T NotMoreThan<T>(this T value, T max) where T: IComparable<T> =>
        Comparer<T>.Default.Compare(value, max) > 0 ? max : value;
}
