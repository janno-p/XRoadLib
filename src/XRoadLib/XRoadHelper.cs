using System;
using System.Linq;
using System.Security.Cryptography;
using XRoadLib.Serialization;
using XRoadLib.Soap;

namespace XRoadLib
{
    public static class XRoadHelper
    {
        public static IMessageFormatter GetMessageFormatter(string contentTypeHeader)
        {
            var contentType = GetMultipartContentType(contentTypeHeader);

            if (!IsMultipartMsg(contentTypeHeader))
                contentType = (contentTypeHeader ?? "").Split(new[] { ';' }, 2).First().Trim();
            else if (contentType?.Equals(ContentTypes.XOP) == true)
                contentType = GetContentTypeStartInfo(contentTypeHeader);

            switch (contentType)
            {
                case null:
                case "":
                case ContentTypes.SOAP:
                    return new SoapMessageFormatter();

                case ContentTypes.SOAP12:
                    return new Soap12MessageFormatter();

                default:
                    throw new InvalidQueryException($"Unknown content type `{contentType}` used for message payload.");
            }
        }

        private static string GetContentTypeStartInfo(string contentTypeHeader) =>
            ExtractValue("start-info=", contentTypeHeader, ";")?.Trim().Trim('"');

        public static string GetMultipartContentType(string contentType) =>
            ExtractValue("type=", contentType, ";")?.Trim().Trim('"');

        public static bool IsMultipartMsg(string contentType) =>
            (contentType ?? "").ToLower().Contains("multipart/related");

        public static string GetContentTypeHeader(string contentType) =>
            $"{contentType}; charset={XRoadEncoding.Utf8.WebName}";

        public static string ExtractValue(string key, string keyValuePair, string separator)
        {
            if (string.IsNullOrEmpty(keyValuePair))
                return null;

            // Mis positsioonilt küsitud key üldse hakkab ..
            var indexOfKey = keyValuePair.ToLower().IndexOf(key.ToLower(), StringComparison.Ordinal);
            if (indexOfKey < 0)
                return null;

            var fromIndex = indexOfKey + key.Length;
            var toIndex = keyValuePair.Length;
            if (separator != null && keyValuePair.IndexOf(separator, fromIndex, StringComparison.Ordinal) > -1)
                toIndex = keyValuePair.IndexOf(separator, fromIndex, StringComparison.Ordinal);

            return keyValuePair.Substring(fromIndex, toIndex - fromIndex).Trim();
        }

        public static string GenerateRequestID()
        {
            const int randomLength = 32;
            const int nonceLength = (int)(4.0d / 3.0d * randomLength);

            var random = new byte[randomLength];

            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(random);

            var nch = new char[nonceLength + 2];
            Convert.ToBase64CharArray(random, 0, randomLength, nch, 0);

            return new string(nch);
        }
    }
}