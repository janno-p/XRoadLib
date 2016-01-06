using System;
using System.Text.RegularExpressions;

namespace XRoadLib
{
    public static class NamespaceHelper
    {
        public const string HTTP = "http://schemas.xmlsoap.org/soap/http";
        public const string MIME = "http://schemas.xmlsoap.org/wsdl/mime/";
        public const string SOAP = "http://schemas.xmlsoap.org/wsdl/soap/";
        public const string SOAP_ENC = "http://schemas.xmlsoap.org/soap/encoding/";
        public const string SOAP_ENV = "http://schemas.xmlsoap.org/soap/envelope/";
        public const string WSDL = "http://schemas.xmlsoap.org/wsdl/";
        public const string XML = "http://www.w3.org/XML/1998/namespace";
        public const string XMLNS = "http://www.w3.org/2000/xmlns/";
        public const string XMIME = "http://www.w3.org/2005/05/xmlmime";
        public const string XOP = "http://www.w3.org/2004/08/xop/include";
        public const string XROAD = "http://x-road.ee/xsd/x-road.xsd";
        public const string XSD = "http://www.w3.org/2001/XMLSchema";
        public const string XSI = "http://www.w3.org/2001/XMLSchema-instance";
        public const string XTEE = "http://x-tee.riik.ee/xsd/xtee.xsd";

        public static string GetXRoadNamespace(XRoadProtocol protocol)
        {
            switch (protocol)
            {
                case XRoadProtocol.Version20:
                    return XTEE;

                case XRoadProtocol.Version31:
                    return XROAD;

                default:
                    throw new ArgumentException("Invalid protocol version.", nameof(protocol));
            }
        }

        public static string GetProducerNamespace(string producerName, XRoadProtocol protocol)
        {
            switch (protocol)
            {
                case XRoadProtocol.Version20:
                    return $"http://producers.{producerName}.xtee.riik.ee/producer/{producerName}";

                case XRoadProtocol.Version31:
                    return $"http://{producerName}.x-road.ee/producer/";

                default:
                    throw new ArgumentException("Invalid protocol version.", nameof(protocol));
            }
        }

        public static string GetNamespaceBase(string @namespace, XRoadProtocol protocol)
        {
            string pattern;

            switch (protocol)
            {
                case XRoadProtocol.Version20:
                    pattern = @"(http\:\/\/producers.\w.xtee.riik.ee/producer/\w)/.+";
                    break;

                case XRoadProtocol.Version31:
                    pattern = @"(http://\w.x-road.ee/producer/)/.+";
                    break;

                default:
                    return null;
            }

            var match = Regex.Match(@namespace, pattern);

            return match.Success ? match.Groups[1].Value : null;
        }
    }
}