using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace K4os.Fx;

/// <summary>
/// Minimal templating engine allowing for <c>{variable}</c> expansions which is more useful than
/// <see cref="M:String.Format()"/>'s <c>{0}</c>. The difference between this and string
/// interpolation is the fact that it can be done on user strings, not only on precompiled ones.
/// </summary>
public static class Template
{
    // Prevents circular expansion, allowing quite deep nesting at the same time.
    private const int MaximumExpansionDepth = 1024;

    private static readonly Regex ExpandableToken = new Regex(
        @"\{(?<name>[\w\._]+)}",
        RegexOptions.ExplicitCapture | RegexOptions.Compiled);

    /// <summary>Returns macro resolver which uses composition of other resolvers.</summary>
    /// <param name="resolvers">Collection of resolvers. Please note that they get processed in order.</param>
    /// <returns>Resolver function.</returns>
    public static Func<string, object?> ResolveMany(params Func<string, object?>[] resolvers) =>
        name => resolvers.Select(r => r(name)).FirstOrDefault(o => o is not null);

    /// <summary>Returns macro resolver which uses dictionary to resolve macros.</summary>
    /// <param name="data">Dictionary.</param>
    /// <returns>Resolver function.</returns>
    public static Func<string, object?> ResolveDictionary<T>(IDictionary<string, T> data) =>
        name => data.TryGetValue(name, out var result) ? result : null;

    /// <summary>Returns macro resolver which uses properties and fields of any object to resolve macros.</summary>
    /// <param name="data">Data object.</param>
    /// <param name="ignoreCase">if <c>true</c> case is ignored when resolving property names.</param>
    /// <returns>Resolver function.</returns>
    public static Func<string, object?> ResolveProperty(object data, bool ignoreCase = false)
    {
        var typeInfo = data.GetType().GetTypeInfo();
        var bindingFlags =
            (ignoreCase ? BindingFlags.IgnoreCase : BindingFlags.Default) |
            BindingFlags.FlattenHierarchy |
            BindingFlags.Public |
            BindingFlags.Instance;

        // ReSharper disable once ConvertIfStatementToReturnStatement
        // ReSharper disable once UseNullPropagation
        return name => {
            var property = typeInfo.GetProperty(name, bindingFlags);
            if (property != null)
            {
                return property.GetValue(data, null);
            }

            var field = typeInfo.GetField(name, bindingFlags);
            if (field != null)
            {
                return field.GetValue(data);
            }

            return null;
        };
    }

    /// <summary>Returns macro resolver which uses regex Match result with named groups.</summary>
    /// <param name="match"></param>
    /// <returns>Resolver function.</returns>
    public static Func<string, string?> ResolveMatch(Match match) =>
        name => match.Groups[name] switch { { Success: true } g => g.Value, _ => null };

    /// <summary>Expands the string.</summary>
    /// <param name="input">The input.</param>
    /// <param name="data">The data used for expansion.</param>
    /// <returns>Expanded string.</returns>
    public static string Expand<T>(string input, IDictionary<string, T> data) =>
        Expand(input, ResolveDictionary(data));

    /// <summary>Expands the string. </summary>
    /// <param name="input">The input.</param>
    /// <param name="data">The data used for expansion.</param>
    /// <returns>Expanded string.</returns>
    public static string Expand(this string input, object data) =>
        Expand(input, ResolveProperty(data));

    /// <summary>Expands the string.</summary>
    /// <param name="input">The input.</param>
    /// <param name="match">Result of regex Match with named groups.</param>
    /// <returns>Expanded string.</returns>
    public static string Expand(this string input, Match match) =>
        Expand(input, ResolveMatch(match));

    /// <summary>Expands the string.</summary>
    /// <param name="input">The input.</param>
    /// <param name="resolver">The resolver.</param>
    /// <returns>Expanded string.</returns>
    public static string Expand(this string input, Func<string, object?> resolver) =>
        Expand(input, x => resolver(x)?.ToString());

    /// <summary>Expands the string.</summary>
    /// <param name="input">The input.</param>
    /// <param name="resolver">The resolver.</param>
    /// <returns>Expanded string.</returns>
    public static string Expand(this string input, Func<string, string?> resolver) =>
        Expand(input, resolver, 0);

    private static string Expand(string input, Func<string, string?> resolver, int depth)
    {
        if (depth >= MaximumExpansionDepth)
        {
            return input;
        }

        StringBuilder? result = null;

        var m = ExpandableToken.Match(input);
        var startIndex = 0;

        while (m.Success)
        {
            result ??= new StringBuilder();

            result.Append(input, startIndex, m.Index - startIndex);
            var name = m.Groups["name"].Value;
            var value = resolver(name);

            result.Append(value is null ? m.Value : Expand(value, resolver, depth + 1));

            startIndex = m.Index + m.Length;

            m = m.NextMatch();
        }

        if (result is null)
        {
            return input; // no tokens found
        }

        result.Append(input, startIndex, input.Length - startIndex);

        return result.ToString();
    }
}
