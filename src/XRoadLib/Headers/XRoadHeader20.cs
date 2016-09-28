using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Styles;

namespace XRoadLib.Headers
{
    /// <summary>
    /// Details of X-Road message protocol version 2.0 header.
    /// </summary>
    public class XRoadHeader20 : IXRoadHeader, IXRoadHeader20
    {
        private static readonly XName stringTypeName = XName.Get("string", NamespaceConstants.XSD);
        private static readonly XName booleanTypeName = XName.Get("boolean", NamespaceConstants.XSD);
        private static readonly XName base64TypeName = XName.Get("base64", NamespaceConstants.XSD);
        private static readonly Func<string, Func<XRoadHeader20, object>, XName, Tuple<string, Func<XRoadHeader20, object>, XName>> createTuple = (a, b, c) => new Tuple<string, Func<XRoadHeader20, object>, XName>(a, b, c);
        private static readonly ICollection<Tuple<string, Func<XRoadHeader20, object>, XName>> elementMappings = new []
        {
            createTuple("asutus", x => x.Asutus, stringTypeName),
            createTuple("andmekogu", x => x.Andmekogu, stringTypeName),
            createTuple("isikukood", x => x.Isikukood, stringTypeName),
            createTuple("toimik", x => x.Toimik, stringTypeName),
            createTuple("nimi", x => x.Nimi, stringTypeName),
            createTuple("ametnik", x => x.Ametnik, stringTypeName),
            createTuple("id", x => x.Id, stringTypeName),
            createTuple("allasutus", x => x.Allasutus, stringTypeName),
            createTuple("amet", x => x.Amet, stringTypeName),
            createTuple("ametniknimi", x => x.AmetnikNimi, stringTypeName),
            createTuple("asynkroonne", x => x.Asünkroonne, booleanTypeName),
            createTuple("autentija", x => x.Autentija, stringTypeName),
            createTuple("makstud", x => x.Makstud, stringTypeName),
            createTuple("salastada", x => x.Salastada, stringTypeName),
            createTuple("salastada_sertifikaadiga", x => x.SalastadaSertifikaadiga, base64TypeName),
            createTuple("salastatud", x => x.Salastatud, stringTypeName),
            createTuple("salastatud_sertifikaadiga", x => x.SalastatudSertifikaadiga, stringTypeName)
        };

        private readonly HeaderDefinition definition;
        private readonly Style style;

        /// <summary>
        /// Identifies X-Road client.
        /// </summary>
        public XRoadClientIdentifier Client { get; set; }

        /// <summary>
        /// Identifies X-Road operation.
        /// </summary>
        public XRoadServiceIdentifier Service { get; set; }

        /// <summary>
        /// Identifies user who sent X-Road message.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Operation specific identifier for the X-Road message.
        /// </summary>
        public string Issue { get; set; }

        /// <summary>
        /// Protocol version of the X-Road message.
        /// </summary>
        string IXRoadHeader.ProtocolVersion => "2.0";

        /// <summary>
        /// Asutuse DNS-nimi.
        /// </summary>
        public string Asutus => Client.MemberCode;

        /// <summary>
        /// Andmekogu DNS-nimi.
        /// </summary>
        public string Andmekogu => Service?.SubsystemCode;

        /// <summary>
        /// Teenuse kasutaja isikukood, millele eelneb kahekohaline maa kood.
        /// Nt. EE37702026518.
        /// </summary>
        public string Isikukood => UserId;

        /// <summary>
        /// Teenuse väljakutsega seonduva toimiku number (mittekohustuslik)
        /// </summary>
        public string Toimik => Issue;

        /// <summary>
        /// Kutsutava teenuse nimi.
        /// </summary>
        public string Nimi => Service?.ToFullName();

        /// <summary>
        /// Unique id for the X-Road message.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Teenuse kasutaja Eesti isikukood.
        /// </summary>
        public string Ametnik { get; set; }

        /// <summary>
        /// Asutuse registrikood, mille nimel teenust kasutatakse (kasutusel juriidilise isiku portaalis).
        /// </summary>
        public string Allasutus { get; set; }

        /// <summary>
        /// Teenuse kasutaja ametikoht.
        /// </summary>
        public string Amet { get; set; }

        /// <summary>
        /// Teenuse kasutaja nimi.
        /// </summary>
        public string AmetnikNimi { get; set; }

        /// <summary>
        /// Teenuse kasutamise asünkroonsus.Kui väärtus on "true", siis sooritab turvaserver päringu asünkroonselt.
        /// </summary>
        public bool? Asünkroonne { get; set; }

        /// <summary>
        /// Teenuse kasutaja autentimise viis.Võimalikud variandid on:
        /// • ID – ID-kaardiga autenditud;
        /// • SERT – muu sertifikaadiga autenditud;
        /// • PANK – panga kaudu autenditud;
        /// • PAROOL – kasutajatunnuse ja parooliga autenditud.
        /// Autentimise viisi järel võib sulgudes olla täpsustus (näiteks panga kaudu autentimisel panga tunnus infosüsteemis).
        /// </summary>
        public string Autentija { get; set; }

        /// <summary>
        /// Teenuse kasutamise eest makstud summa.
        /// </summary>
        public string Makstud { get; set; }

        /// <summary>
        /// Kui asutusele on X-tee keskuse poolt antud päringute salastamise õigus ja andmekogu on nõus päringut
        /// salastama, siis selle elemendi olemasolul päringu päises andmekogu turvaserver krüpteerib päringu logi,
        /// kasutades selleks X-tee keskuse salastusvõtit.
        /// </summary>
        public string Salastada { get; set; }

        /// <summary>
        /// Päringu sooritaja ID-kaardi autentimissertifikaat DER-kujul base64 kodeerituna. Selle elemendi olemasolu
        /// päringu päises väljendab soovi päringu logi salastamiseks asutuse turvaserveris päringu sooritaja ID-kaardi
        /// autentimisvõtmega. Seda välja kasutatakse ainult kodaniku päringute portaalis.
        /// </summary>
        public string SalastadaSertifikaadiga { get; set; }

        /// <summary>
        /// Kui päringu välja päises oli element salastada ja päringulogi salastamine õnnestus, siis vastuse päisesse
        /// lisatakse tühi element salastatud.
        /// </summary>
        public string Salastatud { get; set; }

        /// <summary>
        /// Kui päringu päises oli element salastada_sertifikaadiga ja päringulogi salastamine õnnestus, siis vastuse päisesesse
        /// lisatakse tühi element salastatud_sertifikaadiga.
        /// </summary>
        public string SalastatudSertifikaadiga { get; set; }

        /// <summary>
        /// Initialize new X-Road message protocol version 2.0 header object.
        /// </summary>
        public XRoadHeader20(HeaderDefinition definition, Style style)
        {
            this.definition = definition;
            this.style = style;
        }

        /// <summary>
        /// Try to read current position in XML reader as X-Road header element.
        /// </summary>
        public void ReadHeaderValue(XmlReader reader)
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

            throw XRoadException.InvalidQuery($"Unexpected X-Road header element `{reader.GetXName()}`.");
        }

        /// <summary>
        /// Check if all required SOAP headers are present and in correct format.
        /// </summary>
        public void Validate()
        { }

        /// <summary>
        /// Serializes X-Road message SOAP headers to XML.
        /// </summary>
        public void WriteTo(XmlWriter writer)
        {
            if (writer.LookupPrefix(NamespaceConstants.XTEE) == null)
                writer.WriteAttributeString("xmlns", PrefixConstants.XTEE, NamespaceConstants.XMLNS, NamespaceConstants.XTEE);

            foreach (var m in elementMappings)
            {
                var value = m.Item2(this);
                if (definition.RequiredHeaders.Contains(m.Item1) || value != null)
                    style.WriteHeaderElement(writer, m.Item1, NamespaceConstants.XTEE, value, m.Item3);
            }
        }
    }
}