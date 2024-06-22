using System;
using System.Text;

namespace K4os.Fx;

internal static class StringBuilderExtensions
{
	public static StringBuilder AppendJNull(this StringBuilder builder) =>
		builder.Append("null");

	public static StringBuilder AppendJNumber(this StringBuilder builder, double value) =>
		builder.Append(value);

	public static StringBuilder AppendJBool(this StringBuilder builder, bool value) =>
		builder.Append(value ? "true" : "false");

	public static StringBuilder AppendJQuoted(this StringBuilder builder, string value) =>
		builder.Append('"').AppendEscaped(value).Append('"');

	public static StringBuilder AppendJArray<T>(
		this StringBuilder builder,
		IEnumerable<T> values,
		Func<T, object?>? selector = null)
	{
		selector ??= x => x;

		builder.Append('[');
		var first = true;
		foreach (var item in values)
		{
			if (!first) builder.Append(',');
			builder.AppendJAny(selector(item));
			first = false;
		}

		builder.Append(']');

		return builder;
	}

	public static StringBuilder AppendJObject<T>(
		this StringBuilder builder,
		IEnumerable<T> values,
		Func<T, (string, object?)> selector)
	{
		builder.Append('{');
		var first = true;
		foreach (var kv in values)
		{
			var (key, value) = selector(kv);
			if (!first) builder.Append(',');
			builder.AppendJQuoted(key).Append(':').AppendJAny(value);
			first = false;
		}

		return builder;
	}

	public static StringBuilder AppendJObject<T>(
		this StringBuilder builder,
		IEnumerable<KeyValuePair<string, T>> values,
		Func<T, object?>? selector = null)
	{
		selector ??= x => x;
		return builder.AppendJObject(values, kv => (kv.Key, selector(kv.Value)));
	}

	public static StringBuilder AppendJObject<T>(
		this StringBuilder builder,
		IEnumerable<(string Key, T Value)> values,
		Func<T, object?>? selector = null)
	{
		selector ??= x => x;
		return builder.AppendJObject(values, kv => (kv.Key, selector(kv.Value)));
	}

	public static StringBuilder AppendJAny(this StringBuilder builder, object? value) =>
		value switch {
			null => builder.AppendJNull(),
			string s => builder.AppendJQuoted(s),
			double d => builder.AppendJNumber(d),
			bool b => builder.AppendJBool(b),
			IEnumerable<object> a => builder.AppendJArray(a),
			IEnumerable<int> a => builder.AppendJArray(a),
			IEnumerable<long> a => builder.AppendJArray(a),
			IEnumerable<double> a => builder.AppendJArray(a),
			IEnumerable<bool> a => builder.AppendJArray(a),
			IEnumerable<KeyValuePair<string, object>> o => builder.AppendJObject(o),
			IEnumerable<KeyValuePair<string, string>> o => builder.AppendJObject(o),
			IEnumerable<KeyValuePair<string, int>> o => builder.AppendJObject(o),
			IEnumerable<KeyValuePair<string, long>> o => builder.AppendJObject(o),
			IEnumerable<KeyValuePair<string, double>> o => builder.AppendJObject(o),
			IEnumerable<KeyValuePair<string, bool>> o => builder.AppendJObject(o),
			IEnumerable<(string, object)> o => builder.AppendJObject(o),
			IEnumerable<(string, string)> o => builder.AppendJObject(o),
			IEnumerable<(string, int)> o => builder.AppendJObject(o),
			IEnumerable<(string, long)> o => builder.AppendJObject(o),
			IEnumerable<(string, double)> o => builder.AppendJObject(o),
			IEnumerable<(string, bool)> o => builder.AppendJObject(o),
			_ => throw new NotSupportedException($"Unsupported type: {value.GetType()}")
		};
	
	private static StringBuilder AppendEscaped(this StringBuilder builder, string value)
	{
		foreach (var c in value) AppendEscaped(builder, c);
		return builder;
	}

	private static void AppendEscaped(this StringBuilder builder, char value)
	{
		var text = value switch {
			'"' => "\\\"", '\\' => "\\\\", '\b' => "\\b",
			'\f' => "\\f", '\n' => "\\n", '\r' => "\\r", '\t' => "\\t",
			< ' ' => "\\u" + ((int)value).ToString("x4"),
			_ => null,
		};
		if (text is not null) builder.Append(text);
		else builder.Append(value);
	}
}