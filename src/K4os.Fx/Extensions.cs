using System.Runtime.CompilerServices;

namespace K4os.Fx;

public static class Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfNull<T>(
        this T? subject, 
        [CallerArgumentExpression(nameof(subject))] string? expression = null) =>
        subject ?? ThrowArgumentNull<T>(expression);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ThrowArgumentNull<T>(string? expression) => 
        throw new ArgumentNullException(expression);
}
