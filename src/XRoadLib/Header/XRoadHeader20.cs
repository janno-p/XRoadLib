using System.Xml;

namespace XRoadLib.Header
{
    internal class XRoadHeader20 : XRoadHeaderBase, IXRoadHeader20
    {
        public override XRoadProtocol Protocol => XRoadProtocol.Version20;

        public string Asutus => Client.MemberCode;
        public string Andmekogu => Service?.SubsystemCode;
        public string Isikukood => UserId;
        public string Toimik => Issue;
        public string Nimi => Service?.ToFullName();

        public string Ametnik { get; private set; }
        public string Allasutus { get; private set; }
        public string Amet { get; private set; }
        public string AmetnikNimi { get; private set; }
        public bool Asünkroonne { get; private set; }
        public string Autentija { get; private set; }
        public string Makstud { get; private set; }
        public string Salastada { get; private set; }
        public string SalastadaSertifikaadiga { get; private set; }
        public string Salastatud { get; private set; }
        public string SalastatudSertifikaadiga { get; private set; }

        public override void SetHeaderValue(XmlReader reader)
        {
            if (reader.NamespaceURI == NamespaceConstants.XTEE)
            {
                switch (reader.LocalName)
                {
                    case "autentija":
                        Autentija = reader.ReadInnerXml();
                        return;

                    case "ametniknimi":
                        AmetnikNimi = reader.ReadInnerXml();
                        return;

                    case "amet":
                        Amet = reader.ReadInnerXml();
                        return;

                    case "allasutus":
                        Allasutus = reader.ReadInnerXml();
                        return;

                    case "toimik":
                        Issue = reader.ReadInnerXml();
                        return;

                    case "nimi":
                        var service = XRoadServiceIdentifier.FromString(reader.ReadInnerXml());
                        if (Service == null)
                            Service = new XRoadServiceIdentifier();
                        Service.ServiceCode = service.ServiceCode;
                        Service.ServiceVersion = service.ServiceVersion;
                        return;

                    case "id":
                        Id = reader.ReadInnerXml();
                        return;

                    case "isikukood":
                        UserId = reader.ReadInnerXml();
                        return;

                    case "andmekogu":
                        if (Service == null)
                            Service = new XRoadServiceIdentifier();
                        Service.SubsystemCode = reader.ReadInnerXml();
                        return;

                    case "asutus":
                        if (Client == null)
                            Client = new XRoadClientIdentifier();
                        Client.MemberCode = reader.ReadInnerXml();
                        return;

                    case "ametnik":
                        Ametnik = reader.ReadInnerXml();
                        return;

                    case "asynkroonne":
                        var value = reader.ReadInnerXml().ToLower();
                        Asünkroonne = value.Equals("1") || value.Equals("true");
                        return;

                    case "makstud":
                        Makstud = reader.ReadInnerXml();
                        return;

                    case "salastada":
                        Salastada = reader.ReadInnerXml();
                        return;

                    case "salastada_sertifikaadiga":
                        SalastadaSertifikaadiga = reader.ReadInnerXml();
                        return;

                    case "salastatud":
                        Salastatud = reader.ReadInnerXml();
                        return;

                    case "salastatud_sertifikaadiga":
                        SalastatudSertifikaadiga = reader.ReadInnerXml();
                        return;
                }
            }

            throw XRoadException.InvalidQuery("Unexpected X-Road header element `{0}`.", new XmlQualifiedName(reader.LocalName, reader.NamespaceURI));
        }
    }
}