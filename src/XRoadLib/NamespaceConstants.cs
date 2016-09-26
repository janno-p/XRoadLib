using System.Collections.Generic;

namespace XRoadLib
{
    /// <summary>
    /// Collection of predefined namespaces used by X-Road message protocol.
    /// </summary>
    public static class NamespaceConstants
    {
        /// <summary>
        /// Defines HTTP transport protocol.
        /// </summary>
        public const string HTTP = "http://schemas.xmlsoap.org/soap/http";

        /// <summary>
        /// Extends SOAP specification with mime multipart attachments.
        /// </summary>
        public const string MIME = "http://schemas.xmlsoap.org/wsdl/mime/";

        /// <summary>
        /// Namespace for describing SOAP messages.
        /// </summary>
        public const string SOAP = "http://schemas.xmlsoap.org/wsdl/soap/";

        /// <summary>
        /// Defines encoding style to be used with RPC/Encoded SOAP messages.
        /// </summary>
        public const string SOAP_ENC = "http://schemas.xmlsoap.org/soap/encoding/";

        /// <summary>
        /// Defines main parts of SOAP message.
        /// </summary>
        public const string SOAP_ENV = "http://schemas.xmlsoap.org/soap/envelope/";

        /// <summary>
        /// Namespace for describing WSDL documents.
        /// </summary>
        public const string WSDL = "http://schemas.xmlsoap.org/wsdl/";

        /// <summary>
        /// Default XML namespace.
        /// </summary>
        public const string XML = "http://www.w3.org/XML/1998/namespace";

        /// <summary>
        /// Namespace for defining XML namespaces.
        /// </summary>
        public const string XMLNS = "http://www.w3.org/2000/xmlns/";

        /// <summary>
        /// Describes content type for binary content in service descriptions.
        /// </summary>
        public const string XMIME = "http://www.w3.org/2005/05/xmlmime";

        /// <summary>
        /// Connects SOAP message with binary content container (SOAP attachment).
        /// </summary>
        public const string XOP = "http://www.w3.org/2004/08/xop/include";

        /// <summary>
        /// X-Road message protocol version 3.1 namespace.
        /// </summary>
        public const string XROAD = "http://x-road.ee/xsd/x-road.xsd";

        /// <summary>
        /// X-Road message protocol version 4.0 namespace.
        /// </summary>
        public const string XROAD_V4 = "http://x-road.eu/xsd/xroad.xsd";

        /// <summary>
        /// X-Road message protocol version 4.0 identifiers namespace.
        /// </summary>
        public const string XROAD_V4_ID = "http://x-road.eu/xsd/identifiers";

        /// <summary>
        /// X-Road message protocol version 4.0 represented party namespace.
        /// </summary>
        public const string XROAD_V4_REPR = "http://x-road.eu/xsd/representation.xsd";

        /// <summary>
        /// XML Schema definition namespace.
        /// </summary>
        public const string XSD = "http://www.w3.org/2001/XMLSchema";

        /// <summary>
        /// XML Schema serialization namespace.
        /// </summary>
        public const string XSI = "http://www.w3.org/2001/XMLSchema-instance";

        /// <summary>
        /// X-Road message protocol version 2.0 namespace.
        /// </summary>
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
                case HTTP:
                case MIME:
                case SOAP:
                case SOAP_ENC:
                case SOAP_ENV:
                case WSDL:
                case XML:
                case XMLNS:
                case XMIME:
                case XOP:
                case XROAD:
                case XROAD_V4:
                case XROAD_V4_ID:
                case XROAD_V4_REPR:
                case XSD:
                case XSI:
                case XTEE:
                    return namespaceName;
            }

            return null;
        }
    }
}