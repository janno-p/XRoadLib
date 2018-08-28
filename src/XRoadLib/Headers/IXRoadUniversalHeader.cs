namespace XRoadLib.Headers
{
    public interface IXRoadUniversalHeader
    {
        XRoadClientIdentifier Client { get; set; }
        XRoadServiceIdentifier Service { get; set; }

        string UserId { get; set; }
        string Issue { get; set; }
        string Id { get; set; }
        string ProtocolVersion { get; set; }
        string Unit { get; set; }
        string Position { get; set; }
        string UserName { get; set; }
        string Authenticator { get; set; }
        string Paid { get; set; }
        string Encrypt { get; set; }
        string EncryptCert { get; set; }
        string Encrypted { get; set; }
        string EncryptedCert { get; set; }

        bool? Async { get; set; }
    }
}