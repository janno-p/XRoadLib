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
        public const string Http = "http://schemas.xmlsoap.org/soap/http";

        /// <summary>
        /// Extends SOAP specification with mime multipart attachments.
        /// </summary>
        public const string Mime = "http://schemas.xmlsoap.org/wsdl/mime/";

        /// <summary>
        /// Namespace for describing SOAP messages.
        /// </summary>
        public const string Soap = "http://schemas.xmlsoap.org/wsdl/soap/";

        /// <summary>
        /// Namespace for describing SOAP 1.2 messages.
        /// </summary>
        public const string Soap12 = "http://schemas.xmlsoap.org/wsdl/soap12/";

        /// <summary>
        /// Defines encoding style to be used with RPC/Encoded SOAP messages.
        /// </summary>
        public const string SoapEnc = "http://schemas.xmlsoap.org/soap/encoding/";

        /// <summary>
        /// Defines main parts of SOAP message.
        /// </summary>
        public const string SoapEnv = "http://schemas.xmlsoap.org/soap/envelope/";

        /// <summary>
        /// Defines main parts of SOAP 1.2 message.
        /// </summary>
        public const string Soap12Env = "http://www.w3.org/2003/05/soap-envelope";

        /// <summary>
        /// Namespace for describing WSDL documents.
        /// </summary>
        public const string Wsdl = "http://schemas.xmlsoap.org/wsdl/";

        /// <summary>
        /// Default XML namespace.
        /// </summary>
        public const string Xml = "http://www.w3.org/XML/1998/namespace";

        /// <summary>
        /// Namespace for defining XML namespaces.
        /// </summary>
        public const string Xmlns = "http://www.w3.org/2000/xmlns/";

        /// <summary>
        /// Describes content type for binary content in service descriptions.
        /// </summary>
        public const string Xmime = "http://www.w3.org/2005/05/xmlmime";

        /// <summary>
        /// Connects SOAP message with binary content container (SOAP attachment).
        /// </summary>
        public const string Xop = "http://www.w3.org/2004/08/xop/include";

        /// <summary>
        /// X-Road message protocol version 3.1 namespace.
        /// </summary>
        public const string XRoad = "http://x-road.ee/xsd/x-road.xsd";

        /// <summary>
        /// X-Road message protocol version 4.0 namespace.
        /// </summary>
        public const string XRoadV4 = "http://x-road.eu/xsd/xroad.xsd";

        /// <summary>
        /// X-Road message protocol version 4.0 identifiers namespace.
        /// </summary>
        public const string XRoadV4Id = "http://x-road.eu/xsd/identifiers";

        /// <summary>
        /// X-Road message protocol version 4.0 represented party namespace.
        /// </summary>
        public const string XRoadV4Repr = "http://x-road.eu/xsd/representation.xsd";

        /// <summary>
        /// XML Schema definition namespace.
        /// </summary>
        public const string Xsd = "http://www.w3.org/2001/XMLSchema";

        /// <summary>
        /// XML Schema serialization namespace.
        /// </summary>
        public const string Xsi = "http://www.w3.org/2001/XMLSchema-instance";

        /// <summary>
        /// X-Road message protocol version 2.0 namespace.
        /// </summary>
        public const string Xtee = "http://x-tee.riik.ee/xsd/xtee.xsd";

        /// <summary>
        /// List of namespaces which contain definitions for X-Road meta services.
        /// </summary>
        public static readonly ICollection<string> MetaServiceNamespaces = new[]
        {
            Xtee,
            "http://x-rd.net/xsd/xroad.xsd",
            XRoad,
            "http://x-road.eu/xsd/x-road.xsd"
        };

        /// <summary>
        /// Get schema location of specified schema.
        /// </summary>
        public static string GetSchemaLocation(string namespaceName)
        {
            switch (namespaceName)
            {
                case Http:
                case Mime:
                case Soap:
                case Soap12:
                case SoapEnc:
                case SoapEnv:
                case Soap12Env:
                case Wsdl:
                case Xml:
                case Xmlns:
                case Xmime:
                case Xop:
                case XRoad:
                case XRoadV4:
                case XRoadV4Id:
                case XRoadV4Repr:
                case Xsd:
                case Xsi:
                case Xtee:
                    return namespaceName;
            }

            return null;
        }

        /// <summary>
        /// Get preferred global XML namespace prefix of the namespace.
        /// </summary>
        public static string GetPreferredPrefix(string namespaceName)
        {
            switch (namespaceName)
            {
                case Mime:
                    return PrefixConstants.Mime;

                case Soap:
                    return PrefixConstants.Soap;

                case Soap12:
                    return PrefixConstants.Soap12;

                case SoapEnc:
                    return PrefixConstants.SoapEnc;

                case SoapEnv:
                    return PrefixConstants.SoapEnv;

                case Soap12Env:
                    return PrefixConstants.Soap12Env;

                case Wsdl:
                    return PrefixConstants.Wsdl;

                case Xmime:
                    return PrefixConstants.Xmime;

                case Xop:
                    return PrefixConstants.Xop;

                case XRoad:
                case XRoadV4:
                    return PrefixConstants.XRoad;

                case XRoadV4Id:
                    return PrefixConstants.Id;

                case XRoadV4Repr:
                    return PrefixConstants.Repr;

                case Xsd:
                    return PrefixConstants.Xsd;

                case Xsi:
                    return PrefixConstants.Xsi;

                case Xtee:
                    return PrefixConstants.Xtee;
            }

            return null;
        }
    }
}