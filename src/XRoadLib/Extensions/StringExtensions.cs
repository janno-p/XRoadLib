using System;

namespace XRoadLib.Extensions
{
    internal static class StringExtensions
    {
        public static TResult MapNotEmpty<TResult>(this string value, Func<string, TResult> mapper)
        {
            return string.IsNullOrWhiteSpace(value) ? default : mapper(value);
        }
        
        public static string GetStringOrDefault(this string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }
    }
}