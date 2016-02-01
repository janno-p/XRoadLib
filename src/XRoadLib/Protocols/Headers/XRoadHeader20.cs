using System.Xml;

namespace XRoadLib.Protocols.Headers
{
    public class XRoadHeader20 : IXRoadHeader, IXRoadHeader20
    {
        public XRoadClientIdentifier Client { get; set; }
        public XRoadServiceIdentifier Service { get; set; }
        public string UserId { get; set; }
        public string Issue { get; set; }
        string IXRoadHeader.ProtocolVersion => "";

        public string Asutus => Client.MemberCode;
        public string Andmekogu => Service?.SubsystemCode;
        public string Isikukood => UserId;
        public string Toimik => Issue;
        public string Nimi => Service?.ToFullName();
        public string Id { get; set; }

        public string Ametnik { get; set; }
        public string Allasutus { get; set; }
        public string Amet { get; set; }
        public string AmetnikNimi { get; set; }
        public bool? Asünkroonne { get; set; }
        public string Autentija { get; set; }
        public string Makstud { get; set; }
        public string Salastada { get; set; }
        public string SalastadaSertifikaadiga { get; set; }
        public string Salastatud { get; set; }
        public string SalastatudSertifikaadiga { get; set; }

        public void SetHeaderValue(XmlReader reader)
        {
            if (reader.NamespaceURI == NamespaceConstants.XTEE)
            {
                switch (reader.LocalName)
                {
                    case "autentija":
                        Autentija = reader.ReadElementContentAsString();
                        return;

                    case "ametniknimi":
                        AmetnikNimi = reader.ReadElementContentAsString();
                        return;

                    case "amet":
                        Amet = reader.ReadElementContentAsString();
                        return;

                    case "allasutus":
                        Allasutus = reader.ReadElementContentAsString();
                        return;

                    case "toimik":
                        Issue = reader.ReadElementContentAsString();
                        return;

                    case "nimi":
                        var service = XRoadServiceIdentifier.FromString(reader.ReadElementContentAsString());
                        if (Service == null)
                            Service = new XRoadServiceIdentifier();
                        Service.ServiceCode = service.ServiceCode;
                        Service.ServiceVersion = service.ServiceVersion;
                        return;

                    case "id":
                        Id = reader.ReadElementContentAsString();
                        return;

                    case "isikukood":
                        UserId = reader.ReadElementContentAsString();
                        return;

                    case "andmekogu":
                        if (Service == null)
                            Service = new XRoadServiceIdentifier();
                        Service.SubsystemCode = reader.ReadElementContentAsString();
                        return;

                    case "asutus":
                        if (Client == null)
                            Client = new XRoadClientIdentifier();
                        Client.MemberCode = reader.ReadElementContentAsString();
                        return;

                    case "ametnik":
                        Ametnik = reader.ReadElementContentAsString();
                        return;

                    case "asynkroonne":
                        var value = reader.ReadElementContentAsString();
                        Asünkroonne = !string.IsNullOrWhiteSpace(value) && XmlConvert.ToBoolean(value);
                        return;

                    case "makstud":
                        Makstud = reader.ReadElementContentAsString();
                        return;

                    case "salastada":
                        Salastada = reader.ReadElementContentAsString();
                        return;

                    case "salastada_sertifikaadiga":
                        SalastadaSertifikaadiga = reader.ReadElementContentAsString();
                        return;

                    case "salastatud":
                        Salastatud = reader.ReadElementContentAsString();
                        return;

                    case "salastatud_sertifikaadiga":
                        SalastatudSertifikaadiga = reader.ReadElementContentAsString();
                        return;
                }
            }

            throw XRoadException.InvalidQuery("Unexpected X-Road header element `{0}`.", new XmlQualifiedName(reader.LocalName, reader.NamespaceURI));
        }

        public void Validate()
        { }
    }
}