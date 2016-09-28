using System;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;

namespace XRoadLib.Headers
{
    /// <summary>
    /// Details of X-Road message protocol version 2.0 header.
    /// </summary>
    public class XRoadHeader20 : IXRoadHeader, IXRoadHeader20
    {
        private readonly XRoadClientIdentifier client = new XRoadClientIdentifier();
        private readonly XRoadServiceIdentifier service = new XRoadServiceIdentifier();

        /// <summary>
        /// Identifies X-Road client.
        /// </summary>
        XRoadClientIdentifier IXRoadHeader.Client => client;

        /// <summary>
        /// Identifies X-Road operation.
        /// </summary>
        XRoadServiceIdentifier IXRoadHeader.Service => service;

        /// <summary>
        /// Identifies user who sent X-Road message.
        /// </summary>
        string IXRoadHeader.UserId => Isikukood;

        /// <summary>
        /// Operation specific identifier for the X-Road message.
        /// </summary>
        string IXRoadHeader.Issue => Toimik;

        /// <summary>
        /// Protocol version of the X-Road message.
        /// </summary>
        string IXRoadHeader.ProtocolVersion => "2.0";

        /// <summary>
        /// Asutuse DNS-nimi.
        /// </summary>
        public virtual string Asutus { get { return client.MemberCode; } set { client.MemberCode = value; } }

        /// <summary>
        /// Andmekogu DNS-nimi.
        /// </summary>
        public virtual string Andmekogu { get { return service.SubsystemCode; } set { service.SubsystemCode = value; } }

        /// <summary>
        /// Teenuse kasutaja isikukood, millele eelneb kahekohaline maa kood.
        /// Nt. EE37702026518.
        /// </summary>
        public virtual string Isikukood { get; set; }

        /// <summary>
        /// Teenuse väljakutsega seonduva toimiku number (mittekohustuslik)
        /// </summary>
        public virtual string Toimik { get; set; }

        /// <summary>
        /// Kutsutava teenuse nimi.
        /// </summary>
        public virtual string Nimi
        {
            get { return service.ToFullName(); }
            set
            {
                var serviceValue = XRoadServiceIdentifier.FromString(value);
                service.ServiceCode = serviceValue.ServiceCode;
                service.ServiceVersion = serviceValue.ServiceVersion;
            }
        }

        /// <summary>
        /// Unique id for the X-Road message.
        /// </summary>
        public virtual string Id { get; set; }

        /// <summary>
        /// Teenuse kasutaja Eesti isikukood.
        /// </summary>
        public virtual string Ametnik { get; set; }

        /// <summary>
        /// Asutuse registrikood, mille nimel teenust kasutatakse (kasutusel juriidilise isiku portaalis).
        /// </summary>
        public virtual string Allasutus { get; set; }

        /// <summary>
        /// Teenuse kasutaja ametikoht.
        /// </summary>
        public virtual string Amet { get; set; }

        /// <summary>
        /// Teenuse kasutaja nimi.
        /// </summary>
        public virtual string AmetnikNimi { get; set; }

        /// <summary>
        /// Teenuse kasutamise asünkroonsus.Kui väärtus on "true", siis sooritab turvaserver päringu asünkroonselt.
        /// </summary>
        public virtual bool? Asünkroonne { get; set; }

        /// <summary>
        /// Teenuse kasutaja autentimise viis.Võimalikud variandid on:
        /// • ID – ID-kaardiga autenditud;
        /// • SERT – muu sertifikaadiga autenditud;
        /// • PANK – panga kaudu autenditud;
        /// • PAROOL – kasutajatunnuse ja parooliga autenditud.
        /// Autentimise viisi järel võib sulgudes olla täpsustus (näiteks panga kaudu autentimisel panga tunnus infosüsteemis).
        /// </summary>
        public virtual string Autentija { get; set; }

        /// <summary>
        /// Teenuse kasutamise eest makstud summa.
        /// </summary>
        public virtual string Makstud { get; set; }

        /// <summary>
        /// Kui asutusele on X-tee keskuse poolt antud päringute salastamise õigus ja andmekogu on nõus päringut
        /// salastama, siis selle elemendi olemasolul päringu päises andmekogu turvaserver krüpteerib päringu logi,
        /// kasutades selleks X-tee keskuse salastusvõtit.
        /// </summary>
        public virtual string Salastada { get; set; }

        /// <summary>
        /// Päringu sooritaja ID-kaardi autentimissertifikaat DER-kujul base64 kodeerituna. Selle elemendi olemasolu
        /// päringu päises väljendab soovi päringu logi salastamiseks asutuse turvaserveris päringu sooritaja ID-kaardi
        /// autentimisvõtmega. Seda välja kasutatakse ainult kodaniku päringute portaalis.
        /// </summary>
        public virtual string SalastadaSertifikaadiga { get; set; }

        /// <summary>
        /// Kui päringu välja päises oli element salastada ja päringulogi salastamine õnnestus, siis vastuse päisesse
        /// lisatakse tühi element salastatud.
        /// </summary>
        public virtual string Salastatud { get; set; }

        /// <summary>
        /// Kui päringu päises oli element salastada_sertifikaadiga ja päringulogi salastamine õnnestus, siis vastuse päisesesse
        /// lisatakse tühi element salastatud_sertifikaadiga.
        /// </summary>
        public virtual string SalastatudSertifikaadiga { get; set; }

        /// <summary>
        /// Try to read current position in XML reader as X-Road header element.
        /// </summary>
        public virtual void ReadHeaderValue(XmlReader reader)
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
                        Toimik = reader.ReadElementContentAsString();
                        return;

                    case "nimi":
                        Nimi = reader.ReadElementContentAsString();
                        return;

                    case "id":
                        Id = reader.ReadElementContentAsString();
                        return;

                    case "isikukood":
                        Isikukood = reader.ReadElementContentAsString();
                        return;

                    case "andmekogu":
                        Andmekogu = reader.ReadElementContentAsString();
                        return;

                    case "asutus":
                        Asutus = reader.ReadElementContentAsString();
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
        public virtual void Validate()
        { }

        /// <summary>
        /// Serializes X-Road message SOAP headers to XML.
        /// </summary>
        public virtual void WriteTo(XmlWriter writer, IXRoadProtocol protocol)
        {
            if (writer.LookupPrefix(NamespaceConstants.XTEE) == null)
                writer.WriteAttributeString("xmlns", PrefixConstants.XTEE, NamespaceConstants.XMLNS, NamespaceConstants.XTEE);

            Action<string, object, XName> writeHeaderValue = (elementName, value, typeName) =>
            {
                var name = XName.Get(elementName, NamespaceConstants.XTEE);
                if (protocol.HeaderDefinition.RequiredHeaders.Contains(name) || value != null)
                    protocol.Style.WriteHeaderElement(writer, name, value, typeName);
            };

            writeHeaderValue("asutus", Asutus, XmlTypeConstants.String);
            writeHeaderValue("andmekogu", Andmekogu, XmlTypeConstants.String);
            writeHeaderValue("isikukood", Isikukood, XmlTypeConstants.String);
            writeHeaderValue("toimik", Toimik, XmlTypeConstants.String);
            writeHeaderValue("nimi", Nimi, XmlTypeConstants.String);
            writeHeaderValue("ametnik", Ametnik, XmlTypeConstants.String);
            writeHeaderValue("id", Id, XmlTypeConstants.String);
            writeHeaderValue("allasutus", Allasutus, XmlTypeConstants.String);
            writeHeaderValue("amet", Amet, XmlTypeConstants.String);
            writeHeaderValue("ametniknimi", AmetnikNimi, XmlTypeConstants.String);
            writeHeaderValue("asynkroonne", Asünkroonne, XmlTypeConstants.Boolean);
            writeHeaderValue("autentija", Autentija, XmlTypeConstants.String);
            writeHeaderValue("makstud", Makstud, XmlTypeConstants.String);
            writeHeaderValue("salastada", Salastada, XmlTypeConstants.String);
            writeHeaderValue("salastada_sertifikaadiga", SalastadaSertifikaadiga, XmlTypeConstants.Base64);
            writeHeaderValue("salastatud", Salastatud, XmlTypeConstants.String);
            writeHeaderValue("salastatud_sertifikaadiga", SalastatudSertifikaadiga, XmlTypeConstants.String);
        }
    }
}