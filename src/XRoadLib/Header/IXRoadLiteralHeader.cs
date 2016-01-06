using System.Xml.Serialization;

namespace XRoadLib.Header
{
    /// <summary>
    /// Vastab X-Road skeemis defineeritud kompleks-tüübile {http://x-road.ee/xsd/x-road.xsd}hdrstd.
    /// </summary>
    public interface IXRoadLiteralHeader
    {
        /// <summary>
        /// Asutuse DNS-nimi.
        /// </summary>
        [XmlElement("consumer")]
        string Consumer { get; set; }

        /// <summary>
        /// Andmekogu DNS-nimi.
        /// </summary>
        [XmlElement("producer")]
        string Producer { get; set; }

        /// <summary>
        /// Teenuse kasutaja isikukood, millele eelneb kahekohaline maa kood.
        /// Nt. EE37702026518.
        /// </summary>
        [XmlElement("userId")]
        string UserId { get; set; }

        /// <summary>
        /// Teenuse väljakutse nonss (unikaalne identifikaator).
        /// </summary>
        [XmlElement("id")]
        string Id { get; set; }

        /// <summary>
        /// Kutsutava teenuse nimi.
        /// </summary>
        [XmlElement("service")]
        XRoadServiceName Service { get; set; }

        /// <summary>
        /// Teenuse väljakutsega seonduva toimiku number (mittekohustuslik).
        /// </summary>
        [XmlElement("issue")]
        string Issue { get; set; }

        #region Päringu päises täiendavalt kasutatavad elemendid

        /// <summary>
        /// Asutuse registrikood, mille nimel teenust kasutatakse (kasutusel juriidilise isiku portaalis).
        /// </summary>
        [XmlElement("unit")]
        string Unit { get; set; }

        /// <summary>
        /// Teenuse kasutaja ametikoht.
        /// </summary>
        [XmlElement("position")]
        string Position { get; set; }

        /// <summary>
        /// Teenuse kasutaja nimi.
        /// </summary>
        [XmlElement("userName")]
        string UserName { get; set; }

        /// <summary>
        /// Teenuse kasutamise asünkroonsus. Kui väärtus on "true", siis sooritab turvaserver päringu asünkroonselt.
        /// </summary>
        [XmlElement("async")]
        bool Async { get; set; }

        /// <summary>
        /// Teenuse kasutaja autentimise viis. Võimalikud variandid on:
        /// • ID-CARD – ID-kaardiga autenditud;
        /// • CERT – muu sertifikaadiga autenditud;
        /// • EXTERNAL – panga kaudu autenditud;
        /// • PASSWORD – kasutajatunnuse ja parooliga autenditud.
        /// Autentimise viisi järel võib sulgudes olla täpsustus (näiteks panga kaudu autentimisel panga tunnus infosüsteemis).
        /// </summary>
        [XmlElement("authenticator")]
        string Authenticator { get; set; }

        /// <summary>
        /// Teenuse kasutamise eest makstud summa.
        /// </summary>
        [XmlElement("paid")]
        string Paid { get; set; }

        /// <summary>
        /// Kui asutusele on X-tee keskuse poolt antud päringute salastamise õigus ja andmekogu on nõus päringut
        /// salastama, siis selle elemendi olemasolul päringu päises andmekogu turvaserver krüpteerib päringu logi,
        /// kasutades selleks X-tee keskuse salastusvõtit.
        /// </summary>
        [XmlElement("encrypt")]
        string Encrypt { get; set; }

        /// <summary>
        /// Päringu sooritaja ID-kaardi autentimissertifikaat DER-kujul base64 kodeerituna. Selle elemendi olemasolu
        /// päringu päises väljendab soovi päringu logi salastamiseks asutuse turvaserveris päringu sooritaja ID-kaardi
        /// autentimisvõtmega. Seda välja kasutatakse ainult kodaniku päringute portaalis.
        /// </summary>
        [XmlElement("encryptCert")]
        string EncryptCert { get; set; }

        /// <summary>
        /// Kui päringu välja päises oli element encrypt ja päringulogi salastamine õnnestus, siis vastuse päisesse
        /// lisatakse tühi element encrypted.
        /// </summary>
        [XmlElement("encrypted")]
        string Encrypted { get; set; }

        /// <summary>
        /// Kui päringu päises oli element encryptCert ja päringulogi salastamine õnnestus, siis vastuse päisesesse
        /// lisatakse tühi element encryptedCert.
        /// </summary>
        [XmlElement("encryptedCert")]
        string EncryptedCert { get; set; }

        #endregion
    }
}