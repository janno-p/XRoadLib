using System;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;

namespace XRoadLib.Headers
{
    /// <summary>
    /// Details of X-Road message protocol version 3.1 header.
    /// </summary>
    public class XRoadHeader31 : IXRoadHeader, IXRoadHeader31
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
        public virtual string UserId { get; set; }

        /// <summary>
        /// Operation specific identifier for the X-Road message.
        /// </summary>
        public virtual string Issue { get; set; }

        /// <summary>
        /// Protocol version of the X-Road message.
        /// </summary>
        string IXRoadHeader.ProtocolVersion => "3.1";

        /// <summary>
        /// Asutuse DNS-nimi.
        /// </summary>
        public virtual string Consumer  { get { return client.MemberCode; } set { client.MemberCode = value; } }

        /// <summary>
        /// Andmekogu DNS-nimi.
        /// </summary>
        public virtual string Producer { get { return service.SubsystemCode; } set { service.SubsystemCode = value; } }

        /// <summary>
        /// Kutsutava teenuse nimi.
        /// </summary>
        public virtual string ServiceName
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
        /// Asutuse registrikood, mille nimel teenust kasutatakse (kasutusel juriidilise isiku portaalis).
        /// </summary>
        public virtual string Unit { get; set; }

        /// <summary>
        /// Teenuse kasutaja ametikoht.
        /// </summary>
        public virtual string Position { get; set; }

        /// <summary>
        /// Teenuse kasutaja nimi.
        /// </summary>
        public virtual string UserName { get; set; }

        /// <summary>
        /// Teenuse kasutamise asünkroonsus. Kui väärtus on "true", siis sooritab turvaserver päringu asünkroonselt.
        /// </summary>
        public virtual bool? Async { get; set; }

        /// <summary>
        /// Teenuse kasutaja autentimise viis. Võimalikud variandid on:
        /// • ID-CARD – ID-kaardiga autenditud;
        /// • CERT – muu sertifikaadiga autenditud;
        /// • EXTERNAL – panga kaudu autenditud;
        /// • PASSWORD – kasutajatunnuse ja parooliga autenditud.
        /// Autentimise viisi järel võib sulgudes olla täpsustus (näiteks panga kaudu autentimisel panga tunnus infosüsteemis).
        /// </summary>
        public virtual string Authenticator { get; set; }

        /// <summary>
        /// Teenuse kasutamise eest makstud summa.
        /// </summary>
        public virtual string Paid { get; set; }

        /// <summary>
        /// Kui asutusele on X-tee keskuse poolt antud päringute salastamise õigus ja andmekogu on nõus päringut
        /// salastama, siis selle elemendi olemasolul päringu päises andmekogu turvaserver krüpteerib päringu logi,
        /// kasutades selleks X-tee keskuse salastusvõtit.
        /// </summary>
        public virtual string Encrypt { get; set; }

        /// <summary>
        /// Päringu sooritaja ID-kaardi autentimissertifikaat DER-kujul base64 kodeerituna. Selle elemendi olemasolu
        /// päringu päises väljendab soovi päringu logi salastamiseks asutuse turvaserveris päringu sooritaja ID-kaardi
        /// autentimisvõtmega. Seda välja kasutatakse ainult kodaniku päringute portaalis.
        /// </summary>
        public virtual string EncryptCert { get; set; }

        /// <summary>
        /// Kui päringu välja päises oli element encrypt ja päringulogi salastamine õnnestus, siis vastuse päisesse
        /// lisatakse tühi element encrypted.
        /// </summary>
        public virtual string Encrypted { get; set; }

        /// <summary>
        /// Kui päringu päises oli element encryptCert ja päringulogi salastamine õnnestus, siis vastuse päisesesse
        /// lisatakse tühi element encryptedCert.
        /// </summary>
        public virtual string EncryptedCert { get; set; }

        /// <summary>
        /// Try to read current position in XML reader as X-Road header element.
        /// </summary>
        public virtual void ReadHeaderValue(XmlReader reader)
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
                        ServiceName = reader.ReadElementContentAsString();
                        return;

                    case "id":
                        Id = reader.ReadElementContentAsString();
                        return;

                    case "userId":
                        UserId = reader.ReadElementContentAsString();
                        return;

                    case "producer":
                        Producer = reader.ReadElementContentAsString();
                        return;

                    case "consumer":
                        Consumer = reader.ReadElementContentAsString();
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
        public virtual void Validate()
        { }

        /// <summary>
        /// Serializes X-Road message SOAP headers to XML.
        /// </summary>
        public virtual void WriteTo(XmlWriter writer, IXRoadProtocol protocol)
        {
            if (writer.LookupPrefix(NamespaceConstants.XROAD) == null)
                writer.WriteAttributeString("xmlns", PrefixConstants.XROAD, NamespaceConstants.XMLNS, NamespaceConstants.XROAD);

            Action<string, object, XName> writeHeaderValue = (elementName, value, typeName) =>
            {
                var name = XName.Get(elementName, NamespaceConstants.XROAD);
                if (protocol.HeaderDefinition.RequiredHeaders.Contains(name) || value != null)
                    protocol.Style.WriteHeaderElement(writer, name, value, typeName);
            };

            writeHeaderValue("consumer", Consumer, XmlTypeConstants.String);
            writeHeaderValue("producer", Producer, XmlTypeConstants.String);
            writeHeaderValue("userId", UserId, XmlTypeConstants.String);
            writeHeaderValue("issue", Issue, XmlTypeConstants.String);
            writeHeaderValue("service", ServiceName, XmlTypeConstants.String);
            writeHeaderValue("id", Id, XmlTypeConstants.String);
            writeHeaderValue("unit", Unit, XmlTypeConstants.String);
            writeHeaderValue("position", Position, XmlTypeConstants.String);
            writeHeaderValue("userName", UserName, XmlTypeConstants.String);
            writeHeaderValue("async", Async, XmlTypeConstants.Boolean);
            writeHeaderValue("authenticator", Authenticator, XmlTypeConstants.String);
            writeHeaderValue("paid", Paid, XmlTypeConstants.String);
            writeHeaderValue("encrypt", Encrypt, XmlTypeConstants.String);
            writeHeaderValue("encryptCert", EncryptCert, XmlTypeConstants.Base64);
            writeHeaderValue("encrypted", Encrypted, XmlTypeConstants.String);
            writeHeaderValue("encryptedCert", EncryptedCert, XmlTypeConstants.String);
        }
    }
}