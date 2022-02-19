namespace XRoadLib.Extensions;

internal static class StringExtensions
{
    public static TResult? MapNotEmpty<TResult>(this string? value, Func<string, TResult> mapper) =>
        string.IsNullOrWhiteSpace(value) ? default : mapper(value!);

    public static string GetStringOrDefault(this string? value, string defaultValue) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : value!;

    internal static XmlQualifiedName ToXmlQualifiedName(this XName name) =>
        new(name.LocalName, name.NamespaceName);
}