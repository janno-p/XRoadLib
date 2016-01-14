using System.Xml;

namespace XRoadLib.Header
{
    internal class XRoadHeader31 : XRoadHeaderBase, IXRoadHeader31
    {
        public override XRoadProtocol Protocol => XRoadProtocol.Version31;

        public string Consumer => Client.MemberCode;
        public string Producer => Service.SubsystemCode;
        string IXRoadHeader31.Service => Service.ToFullName();

        public string Unit { get; private set; }
        public string Position { get; private set; }
        public string UserName { get; private set; }
        public bool Async { get; private set; }
        public string Authenticator { get; private set; }
        public string Paid { get; private set; }
        public string Encrypt { get; private set; }
        public string EncryptCert { get; private set; }
        public string Encrypted { get; private set; }
        public string EncryptedCert { get; private set; }

        public override void SetHeaderValue(XmlReader reader)
        {
            if (reader.NamespaceURI == NamespaceConstants.XROAD)
            {
                switch (reader.LocalName)
                {
                    case "authenticator":
                        Authenticator = reader.ReadInnerXml();
                        return;

                    case "userName":
                        UserName = reader.ReadInnerXml();
                        return;

                    case "position":
                        Position = reader.ReadInnerXml();
                        return;

                    case "unit":
                        Unit = reader.ReadInnerXml();
                        return;

                    case "issue":
                        Issue = reader.ReadInnerXml();
                        return;

                    case "service":
                        var service = XRoadServiceIdentifier.FromString(reader.ReadInnerXml());
                        if (Service == null)
                            Service = new XRoadServiceIdentifier();
                        Service.ServiceCode = service.ServiceCode;
                        Service.ServiceVersion = service.ServiceVersion;
                        return;

                    case "id":
                        Id = reader.ReadInnerXml();
                        return;

                    case "userId":
                        UserId = reader.ReadInnerXml();
                        return;

                    case "producer":
                        if (Service == null)
                            Service = new XRoadServiceIdentifier();
                        Service.SubsystemCode = reader.ReadInnerXml();
                        return;

                    case "consumer":
                        if (Client == null)
                            Client = new XRoadClientIdentifier();
                        Client.MemberCode = reader.ReadInnerXml();
                        return;

                    case "async":
                        var value = reader.ReadInnerXml().ToLower();
                        Async = value.Equals("1") || value.Equals("true");
                        return;

                    case "paid":
                        Paid = reader.ReadInnerXml();
                        return;

                    case "encrypt":
                        Encrypt = reader.ReadInnerXml();
                        return;

                    case "encryptCert":
                        EncryptCert = reader.ReadInnerXml();
                        return;

                    case "encrypted":
                        Encrypted = reader.ReadInnerXml();
                        return;

                    case "encryptedCert":
                        EncryptedCert = reader.ReadInnerXml();
                        return;
                }
            }

            throw XRoadException.InvalidQuery("Unexpected X-Road header element `{0}`.", new XmlQualifiedName(reader.LocalName, reader.NamespaceURI));
        }
    }
}