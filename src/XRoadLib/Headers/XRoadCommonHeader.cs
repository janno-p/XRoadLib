namespace XRoadLib.Headers
{
    public class XRoadCommonHeader
    {
        public XRoadClientIdentifier Client { get; set; }
        public XRoadServiceIdentifier Service { get; set; }

        public string UserId { get; set; }
        public string Issue { get; set; }
        public string Id { get; set; }
        public string ProtocolVersion { get; set; }
        public string Unit { get; set; }
        public string Position { get; set; }
        public string UserName { get; set; }
        public string Authenticator { get; set; }
        public string Paid { get; set; }
        public string Encrypt { get; set; }
        public string EncryptCert { get; set; }
        public string Encrypted { get; set; }
        public string EncryptedCert { get; set; }

        public bool? Async { get; set; }
    }
}