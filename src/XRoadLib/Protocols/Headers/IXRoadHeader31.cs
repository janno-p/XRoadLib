using System.Xml.Serialization;

namespace XRoadLib.Protocols.Headers
{
    public interface IXRoadHeader31
    {
        /// <summary>
        /// Asutuse DNS-nimi.
        /// </summary>
        [XmlElement("consumer", Namespace = NamespaceConstants.XROAD)]
        string Consumer { get; }

        /// <summary>
        /// Andmekogu DNS-nimi.
        /// </summary>
        [XmlElement("producer", Namespace = NamespaceConstants.XROAD)]
        string Producer { get; }

        /// <summary>
        /// Teenuse kasutaja isikukood, millele eelneb kahekohaline maa kood.
        /// Nt. EE37702026518.
        /// </summary>
        [XmlElement("userId", Namespace = NamespaceConstants.XROAD)]
        string UserId { get; }

        /// <summary>
        /// Teenuse väljakutse nonss (unikaalne identifikaator).
        /// </summary>
        [XmlElement("id", Namespace = NamespaceConstants.XROAD)]
        string Id { get; }

        /// <summary>
        /// Kutsutava teenuse nimi.
        /// </summary>
        [XmlElement("service", Namespace = NamespaceConstants.XROAD)]
        string Service { get; }

        /// <summary>
        /// Teenuse väljakutsega seonduva toimiku number (mittekohustuslik).
        /// </summary>
        [XmlElement("issue", Namespace = NamespaceConstants.XROAD)]
        string Issue { get; }

        /// <summary>
        /// Asutuse registrikood, mille nimel teenust kasutatakse (kasutusel juriidilise isiku portaalis).
        /// </summary>
        [XmlElement("unit", Namespace = NamespaceConstants.XROAD)]
        string Unit { get; }

        /// <summary>
        /// Teenuse kasutaja ametikoht.
        /// </summary>
        [XmlElement("position", Namespace = NamespaceConstants.XROAD)]
        string Position { get; }

        /// <summary>
        /// Teenuse kasutaja nimi.
        /// </summary>
        [XmlElement("userName", Namespace = NamespaceConstants.XROAD)]
        string UserName { get; }

        /// <summary>
        /// Teenuse kasutamise asünkroonsus. Kui väärtus on "true", siis sooritab turvaserver päringu asünkroonselt.
        /// </summary>
        [XmlElement("async", Namespace = NamespaceConstants.XROAD)]
        bool? Async { get; }

        /// <summary>
        /// Teenuse kasutaja autentimise viis. Võimalikud variandid on:
        /// • ID-CARD – ID-kaardiga autenditud;
        /// • CERT – muu sertifikaadiga autenditud;
        /// • EXTERNAL – panga kaudu autenditud;
        /// • PASSWORD – kasutajatunnuse ja parooliga autenditud.
        /// Autentimise viisi järel võib sulgudes olla täpsustus (näiteks panga kaudu autentimisel panga tunnus infosüsteemis).
        /// </summary>
        [XmlElement("authenticator", Namespace = NamespaceConstants.XROAD)]
        string Authenticator { get; }

        /// <summary>
        /// Teenuse kasutamise eest makstud summa.
        /// </summary>
        [XmlElement("paid", Namespace = NamespaceConstants.XROAD)]
        string Paid { get; }

        /// <summary>
        /// Kui asutusele on X-tee keskuse poolt antud päringute salastamise õigus ja andmekogu on nõus päringut
        /// salastama, siis selle elemendi olemasolul päringu päises andmekogu turvaserver krüpteerib päringu logi,
        /// kasutades selleks X-tee keskuse salastusvõtit.
        /// </summary>
        [XmlElement("encrypt", Namespace = NamespaceConstants.XROAD)]
        string Encrypt { get; }

        /// <summary>
        /// Päringu sooritaja ID-kaardi autentimissertifikaat DER-kujul base64 kodeerituna. Selle elemendi olemasolu
        /// päringu päises väljendab soovi päringu logi salastamiseks asutuse turvaserveris päringu sooritaja ID-kaardi
        /// autentimisvõtmega. Seda välja kasutatakse ainult kodaniku päringute portaalis.
        /// </summary>
        [XmlElement("encryptCert", Namespace = NamespaceConstants.XROAD)]
        string EncryptCert { get; }

        /// <summary>
        /// Kui päringu välja päises oli element encrypt ja päringulogi salastamine õnnestus, siis vastuse päisesse
        /// lisatakse tühi element encrypted.
        /// </summary>
        [XmlElement("encrypted", Namespace = NamespaceConstants.XROAD)]
        string Encrypted { get; }

        /// <summary>
        /// Kui päringu päises oli element encryptCert ja päringulogi salastamine õnnestus, siis vastuse päisesesse
        /// lisatakse tühi element encryptedCert.
        /// </summary>
        [XmlElement("encryptedCert", Namespace = NamespaceConstants.XROAD)]
        string EncryptedCert { get; }
    }
}