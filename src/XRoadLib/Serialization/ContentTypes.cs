namespace XRoadLib.Serialization
{
    public static class ContentTypes
    {
        /// <summary>
        /// Content-type of SOAP 1.1 protocol messages.
        /// </summary>
        public const string SOAP = "text/xml";

        /// <summary>
        /// Content-type of SOAP 1.2 protocol messages.
        /// </summary>
        public const string SOAP12 = "application/soap+xml";

        /// <summary>
        /// Content-type of messages with mime/multipart optimization.
        /// </summary>
        public const string XOP = "application/xop+xml";

        /// <summary>
        /// Content-type of mime/multipart messages.
        /// </summary>
        public const string MULTIPART = "multipart/related";
    }
}