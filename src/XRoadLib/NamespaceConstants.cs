using System.Collections.Generic;

namespace XRoadLib
{
    public static class NamespaceConstants
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
        public const string XROAD_V4 = "http://x-road.eu/xsd/xroad.xsd";
        public const string XROAD_V4_ID = "http://x-road.eu/xsd/identifiers";
        public const string XROAD_V4_REPR = "http://x-road.eu/xsd/representation.xsd";
        public const string XSD = "http://www.w3.org/2001/XMLSchema";
        public const string XSI = "http://www.w3.org/2001/XMLSchema-instance";
        public const string XTEE = "http://x-tee.riik.ee/xsd/xtee.xsd";

        /// <summary>
        /// List of namespaces which contain definitions for X-Road meta services.
        /// </summary>
        public static readonly ICollection<string> MetaServiceNamespaces = new[]
        {
            XTEE,
            "http://x-rd.net/xsd/xroad.xsd",
            XROAD,
            "http://x-road.eu/xsd/x-road.xsd"
        };

        /// <summary>
        /// Get schema location of specified schema.
        /// </summary>
        public static string GetSchemaLocation(string namespaceName)
        {
            switch (namespaceName)
            {
                case NamespaceConstants.HTTP:
                case NamespaceConstants.MIME:
                case NamespaceConstants.SOAP:
                case NamespaceConstants.SOAP_ENC:
                case NamespaceConstants.SOAP_ENV:
                case NamespaceConstants.WSDL:
                case NamespaceConstants.XML:
                case NamespaceConstants.XMLNS:
                case NamespaceConstants.XMIME:
                case NamespaceConstants.XOP:
                case NamespaceConstants.XROAD:
                case NamespaceConstants.XROAD_V4:
                case NamespaceConstants.XROAD_V4_ID:
                case NamespaceConstants.XROAD_V4_REPR:
                case NamespaceConstants.XSD:
                case NamespaceConstants.XSI:
                case NamespaceConstants.XTEE:
                    return namespaceName;
            }

            return null;
        }
    }
}