using System;

namespace XRoadLib
{
    public static class PrefixHelper
    {
        private const string XROAD = "xrd";
        private const string XTEE = "xtee";

        public const string MIME = "mime";
        public const string SOAP = "soap";
        public const string SOAP_ENC = "SOAP-ENC";
        public const string SOAP_ENV = "SOAP-ENV";
        public const string TARGET = "tns";
        public const string WSDL = "wsdl";
        public const string XMIME = "xmime";
        public const string XOP = "xop";
        public const string XSD = "xsd";
        public const string XSI = "xsi";

        public static string GetXRoadPrefix(XRoadProtocol protocol)
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
    }
}