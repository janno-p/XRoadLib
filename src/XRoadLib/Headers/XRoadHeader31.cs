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
    /// Details of X-Road message protocol version 3.1 header.
    /// </summary>
    public class XRoadHeader31 : IXRoadHeader, IXRoadHeader31
    {
        private static readonly XName stringTypeName = XName.Get("string", NamespaceConstants.XSD);
        private static readonly XName booleanTypeName = XName.Get("boolean", NamespaceConstants.XSD);
        private static readonly XName base64TypeName = XName.Get("base64", NamespaceConstants.XSD);
        private static readonly Func<string, Func<XRoadHeader31, object>, XName, Tuple<string, Func<XRoadHeader31, object>, XName>> createTuple = (a, b, c) => new Tuple<string, Func<XRoadHeader31, object>, XName>(a, b, c);
        private static readonly ICollection<Tuple<string, Func<XRoadHeader31, object>, XName>> elementMappings = new []
        {
            createTuple("consumer", x => x.Consumer, stringTypeName),
            createTuple("producer", x => x.Producer, stringTypeName),
            createTuple("userId", x => x.UserId, stringTypeName),
            createTuple("issue", x => x.Issue, stringTypeName),
            createTuple("service", x => ((IXRoadHeader31)x).Service, stringTypeName),
            createTuple("id", x => x.Id, stringTypeName),
            createTuple("unit", x => x.Unit, stringTypeName),
            createTuple("position", x => x.Position, stringTypeName),
            createTuple("userName", x => x.UserName, stringTypeName),
            createTuple("async", x => x.Async, booleanTypeName),
            createTuple("authenticator", x => x.Authenticator, stringTypeName),
            createTuple("paid", x => x.Paid, stringTypeName),
            createTuple("encrypt", x => x.Encrypt, stringTypeName),
            createTuple("encryptCert", x => x.EncryptCert, base64TypeName),
            createTuple("encrypted", x => x.Encrypted, stringTypeName),
            createTuple("encryptedCert", x => x.EncryptedCert, stringTypeName),
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
        string IXRoadHeader.ProtocolVersion => "3.1";

        /// <summary>
        /// Asutuse DNS-nimi.
        /// </summary>
        public string Consumer => Client.MemberCode;

        /// <summary>
        /// Andmekogu DNS-nimi.
        /// </summary>
        public string Producer => Service.SubsystemCode;

        /// <summary>
        /// Kutsutava teenuse nimi.
        /// </summary>
        string IXRoadHeader31.Service => Service.ToFullName();

        /// <summary>
        /// Unique id for the X-Road message.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Asutuse registrikood, mille nimel teenust kasutatakse (kasutusel juriidilise isiku portaalis).
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Teenuse kasutaja ametikoht.
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// Teenuse kasutaja nimi.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Teenuse kasutamise asünkroonsus. Kui väärtus on "true", siis sooritab turvaserver päringu asünkroonselt.
        /// </summary>
        public bool? Async { get; set; }

        /// <summary>
        /// Teenuse kasutaja autentimise viis. Võimalikud variandid on:
        /// • ID-CARD – ID-kaardiga autenditud;
        /// • CERT – muu sertifikaadiga autenditud;
        /// • EXTERNAL – panga kaudu autenditud;
        /// • PASSWORD – kasutajatunnuse ja parooliga autenditud.
        /// Autentimise viisi järel võib sulgudes olla täpsustus (näiteks panga kaudu autentimisel panga tunnus infosüsteemis).
        /// </summary>
        public string Authenticator { get; set; }

        /// <summary>
        /// Teenuse kasutamise eest makstud summa.
        /// </summary>
        public string Paid { get; set; }

        /// <summary>
        /// Kui asutusele on X-tee keskuse poolt antud päringute salastamise õigus ja andmekogu on nõus päringut
        /// salastama, siis selle elemendi olemasolul päringu päises andmekogu turvaserver krüpteerib päringu logi,
        /// kasutades selleks X-tee keskuse salastusvõtit.
        /// </summary>
        public string Encrypt { get; set; }

        /// <summary>
        /// Päringu sooritaja ID-kaardi autentimissertifikaat DER-kujul base64 kodeerituna. Selle elemendi olemasolu
        /// päringu päises väljendab soovi päringu logi salastamiseks asutuse turvaserveris päringu sooritaja ID-kaardi
        /// autentimisvõtmega. Seda välja kasutatakse ainult kodaniku päringute portaalis.
        /// </summary>
        public string EncryptCert { get; set; }

        /// <summary>
        /// Kui päringu välja päises oli element encrypt ja päringulogi salastamine õnnestus, siis vastuse päisesse
        /// lisatakse tühi element encrypted.
        /// </summary>
        public string Encrypted { get; set; }

        /// <summary>
        /// Kui päringu päises oli element encryptCert ja päringulogi salastamine õnnestus, siis vastuse päisesesse
        /// lisatakse tühi element encryptedCert.
        /// </summary>
        public string EncryptedCert { get; set; }

        /// <summary>
        /// Initialize new X-Road message protocol version 3.1 header object.
        /// </summary>
        public XRoadHeader31(HeaderDefinition definition, Style style)
        {
            this.definition = definition;
            this.style = style;
        }

        /// <summary>
        /// Try to read current position in XML reader as X-Road header element.
        /// </summary>
        public void ReadHeaderValue(XmlReader reader)
        {
            if (reader.NamespaceURI == NamespaceConstants.XROAD)
            {
                switch (reader.LocalName)
                {
                    case "authenticator":
                        Authenticator = reader.ReadElementContentAsString();
                        return;

                    case "userName":
                        UserName = reader.ReadElementContentAsString();
                        return;

                    case "position":
                        Position = reader.ReadElementContentAsString();
                        return;

                    case "unit":
                        Unit = reader.ReadElementContentAsString();
                        return;

                    case "issue":
                        Issue = reader.ReadElementContentAsString();
                        return;

                    case "service":
                        var service = XRoadServiceIdentifier.FromString(reader.ReadElementContentAsString());
                        if (Service == null)
                            Service = new XRoadServiceIdentifier();
                        Service.ServiceCode = service.ServiceCode;
                        Service.ServiceVersion = service.ServiceVersion;
                        return;

                    case "id":
                        Id = reader.ReadElementContentAsString();
                        return;

                    case "userId":
                        UserId = reader.ReadElementContentAsString();
                        return;

                    case "producer":
                        if (Service == null)
                            Service = new XRoadServiceIdentifier();
                        Service.SubsystemCode = reader.ReadElementContentAsString();
                        return;

                    case "consumer":
                        if (Client == null)
                            Client = new XRoadClientIdentifier();
                        Client.MemberCode = reader.ReadElementContentAsString();
                        return;

                    case "async":
                        var value = reader.ReadElementContentAsString();
                        Async = !string.IsNullOrWhiteSpace(value) && XmlConvert.ToBoolean(value);
                        return;

                    case "paid":
                        Paid = reader.ReadElementContentAsString();
                        return;

                    case "encrypt":
                        Encrypt = reader.ReadElementContentAsString();
                        return;

                    case "encryptCert":
                        EncryptCert = reader.ReadElementContentAsString();
                        return;

                    case "encrypted":
                        Encrypted = reader.ReadElementContentAsString();
                        return;

                    case "encryptedCert":
                        EncryptedCert = reader.ReadElementContentAsString();
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
            if (writer.LookupPrefix(NamespaceConstants.XROAD) == null)
                writer.WriteAttributeString("xmlns", PrefixConstants.XROAD, NamespaceConstants.XMLNS, NamespaceConstants.XROAD);

            foreach (var m in elementMappings)
            {
                var value = m.Item2(this);
                if (definition.RequiredHeaders.Contains(m.Item1) || value != null)
                    style.WriteHeaderElement(writer, m.Item1, NamespaceConstants.XROAD, value, m.Item3);
            }
        }
    }
}