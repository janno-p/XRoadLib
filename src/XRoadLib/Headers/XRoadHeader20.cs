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
    public class XRoadHeader20 : IXRoadHeader, IXRoadHeader<XRoadHeader20>, IXRoadHeader20
    {
        private readonly XRoadClientIdentifier client = new XRoadClientIdentifier();
        private readonly XRoadServiceIdentifier service = new XRoadServiceIdentifier();

        /// <inheritdoc />
        XRoadClientIdentifier IXRoadHeader.Client => client;

        /// <inheritdoc />
        XRoadServiceIdentifier IXRoadHeader.Service => service;

        /// <inheritdoc />
        string IXRoadHeader.UserId => Isikukood;

        /// <inheritdoc />
        string IXRoadHeader.Issue => Toimik;

        /// <inheritdoc />
        string IXRoadHeader.ProtocolVersion => "2.0";

        /// <inheritdoc />
        public virtual string Asutus
        {
            get => client.MemberCode;
            set => client.MemberCode = value;
        }

        /// <inheritdoc />
        public virtual string Andmekogu
        {
            get => service.SubsystemCode;
            set => service.SubsystemCode = value;
        }

        /// <inheritdoc />
        public virtual string Isikukood { get; set; }

        /// <inheritdoc />
        public virtual string Toimik { get; set; }

        /// <inheritdoc />
        public virtual string Nimi
        {
            get => service.ToFullName();
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

        /// <inheritdoc />
        public virtual string Ametnik { get; set; }

        /// <inheritdoc />
        public virtual string Allasutus { get; set; }

        /// <inheritdoc />
        public virtual string Amet { get; set; }

        /// <inheritdoc />
        public virtual string AmetnikNimi { get; set; }

        /// <inheritdoc />
        public virtual bool? Asünkroonne { get; set; }

        /// <inheritdoc />
        public virtual string Autentija { get; set; }

        /// <inheritdoc />
        public virtual string Makstud { get; set; }

        /// <inheritdoc />
        public virtual string Salastada { get; set; }

        /// <inheritdoc />
        public virtual string SalastadaSertifikaadiga { get; set; }

        /// <inheritdoc />
        public virtual string Salastatud { get; set; }

        /// <inheritdoc />
        public virtual string SalastatudSertifikaadiga { get; set; }

        /// <inheritdoc />
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

            throw new InvalidXRoadQueryException($"Unexpected X-Road header element `{reader.GetXName()}`.");
        }

        /// <inheritdoc />
        public virtual void Validate()
        { }

        /// <inheritdoc />
        public virtual void WriteTo(XmlWriter writer, Style style, HeaderDefinition definition)
        {
            if (writer.LookupPrefix(NamespaceConstants.XTEE) == null)
                writer.WriteAttributeString("xmlns", PrefixConstants.XTEE, NamespaceConstants.XMLNS, NamespaceConstants.XTEE);

            void WriteHeaderValue(string elementName, object value, XName typeName)
            {
                var name = XName.Get(elementName, NamespaceConstants.XTEE);
                if (definition.RequiredHeaders.Contains(name) || value != null) style.WriteHeaderElement(writer, name, value, typeName);
            }

            WriteHeaderValue("asutus", Asutus, XmlTypeConstants.String);
            WriteHeaderValue("andmekogu", Andmekogu, XmlTypeConstants.String);
            WriteHeaderValue("isikukood", Isikukood, XmlTypeConstants.String);
            WriteHeaderValue("toimik", Toimik, XmlTypeConstants.String);
            WriteHeaderValue("nimi", Nimi, XmlTypeConstants.String);
            WriteHeaderValue("ametnik", Ametnik, XmlTypeConstants.String);
            WriteHeaderValue("id", Id, XmlTypeConstants.String);
            WriteHeaderValue("allasutus", Allasutus, XmlTypeConstants.String);
            WriteHeaderValue("amet", Amet, XmlTypeConstants.String);
            WriteHeaderValue("ametniknimi", AmetnikNimi, XmlTypeConstants.String);
            WriteHeaderValue("asynkroonne", Asünkroonne, XmlTypeConstants.Boolean);
            WriteHeaderValue("autentija", Autentija, XmlTypeConstants.String);
            WriteHeaderValue("makstud", Makstud, XmlTypeConstants.String);
            WriteHeaderValue("salastada", Salastada, XmlTypeConstants.String);
            WriteHeaderValue("salastada_sertifikaadiga", SalastadaSertifikaadiga, XmlTypeConstants.Base64);
            WriteHeaderValue("salastatud", Salastatud, XmlTypeConstants.String);
            WriteHeaderValue("salastatud_sertifikaadiga", SalastatudSertifikaadiga, XmlTypeConstants.String);
        }

        public XRoadHeader20 InitFrom(XRoadCommonHeader commonHeader)
        {
            Asutus = commonHeader.Client.MemberCode;
            Andmekogu = commonHeader.Service.SubsystemCode;
            Isikukood = commonHeader.UserId;
            Toimik = commonHeader.Issue;
            Nimi = commonHeader.Service.ToFullName();
            Id = commonHeader.Id;
            Allasutus = commonHeader.Unit;
            Amet = commonHeader.Position;
            AmetnikNimi = commonHeader.UserName;
            Asünkroonne = commonHeader.Async;
            Autentija = commonHeader.Authenticator;
            Makstud = commonHeader.Paid;
            Salastada = commonHeader.Encrypt;
            SalastadaSertifikaadiga = commonHeader.EncryptCert;
            Salastatud = commonHeader.Encrypted;
            SalastatudSertifikaadiga = commonHeader.EncryptedCert;

            return this;
        }
    }
}