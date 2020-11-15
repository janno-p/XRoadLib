using System.Xml.Serialization;

namespace XRoadLib.Headers
{
    /// <summary>
    /// X-Road message protocol version 2.0 SOAP header elements.
    /// </summary>
    public interface IXRoadHeader20
    {
        /// <summary>
        /// Asutuse DNS-nimi.
        /// </summary>
        [XmlElement("asutus", Namespace = NamespaceConstants.Xtee)]
        string Asutus { get; }

        /// <summary>
        /// Andmekogu DNS-nimi.
        /// </summary>
        [XmlElement("andmekogu", Namespace = NamespaceConstants.Xtee)]
        string Andmekogu { get; }

        /// <summary>
        /// Teenuse kasutaja isikukood, millele eelneb kahekohaline maa kood.
        /// Nt. EE37702026518.
        /// </summary>
        [XmlElement("isikukood", Namespace = NamespaceConstants.Xtee)]
        string Isikukood { get; }

        /// <summary>
        /// Teenuse kasutaja Eesti isikukood.
        /// </summary>
        [XmlElement("ametnik", Namespace = NamespaceConstants.Xtee)]
        string Ametnik { get; }

        /// <summary>
        /// Teenuse väljakutse nonss (unikaalne identifikaator).
        /// </summary>
        [XmlElement("id", Namespace = NamespaceConstants.Xtee)]
        string Id { get; }

        /// <summary>
        /// Kutsutava teenuse nimi.
        /// </summary>
        [XmlElement("nimi", Namespace = NamespaceConstants.Xtee)]
        string Nimi { get; }

        /// <summary>
        /// Teenuse väljakutsega seonduva toimiku number (mittekohustuslik)
        /// </summary>
        [XmlElement("toimik", Namespace = NamespaceConstants.Xtee)]
        string Toimik { get; }

        /// <summary>
        /// Asutuse registrikood, mille nimel teenust kasutatakse (kasutusel juriidilise isiku portaalis).
        /// </summary>
        [XmlElement("allasutus", Namespace = NamespaceConstants.Xtee)]
        string Allasutus { get; }

        /// <summary>
        /// Teenuse kasutaja ametikoht.
        /// </summary>
        [XmlElement("amet", Namespace = NamespaceConstants.Xtee)]
        string Amet { get; }

        /// <summary>
        /// Teenuse kasutaja nimi.
        /// </summary>
        [XmlElement("ametniknimi", Namespace = NamespaceConstants.Xtee)]
        string AmetnikNimi { get; }

        /// <summary>
        /// Teenuse kasutamise asünkroonsus.Kui väärtus on "true", siis sooritab turvaserver päringu asünkroonselt.
        /// </summary>
        [XmlElement("asynkroonne", Namespace = NamespaceConstants.Xtee)]
        bool? Asünkroonne { get; }

        /// <summary>
        /// Teenuse kasutaja autentimise viis.Võimalikud variandid on:
        /// • ID – ID-kaardiga autenditud;
        /// • SERT – muu sertifikaadiga autenditud;
        /// • PANK – panga kaudu autenditud;
        /// • PAROOL – kasutajatunnuse ja parooliga autenditud.
        /// Autentimise viisi järel võib sulgudes olla täpsustus (näiteks panga kaudu autentimisel panga tunnus infosüsteemis).
        /// </summary>
        [XmlElement("autentija", Namespace = NamespaceConstants.Xtee)]
        string Autentija { get; }

        /// <summary>
        /// Teenuse kasutamise eest makstud summa.
        /// </summary>
        [XmlElement("makstud", Namespace = NamespaceConstants.Xtee)]
        string Makstud { get; }

        /// <summary>
        /// Kui asutusele on X-tee keskuse poolt antud päringute salastamise õigus ja andmekogu on nõus päringut
        /// salastama, siis selle elemendi olemasolul päringu päises andmekogu turvaserver krüpteerib päringu logi,
        /// kasutades selleks X-tee keskuse salastusvõtit.
        /// </summary>
        [XmlElement("salastada", Namespace = NamespaceConstants.Xtee)]
        string Salastada { get; }

        /// <summary>
        /// Päringu sooritaja ID-kaardi autentimissertifikaat DER-kujul base64 kodeerituna. Selle elemendi olemasolu
        /// päringu päises väljendab soovi päringu logi salastamiseks asutuse turvaserveris päringu sooritaja ID-kaardi
        /// autentimisvõtmega. Seda välja kasutatakse ainult kodaniku päringute portaalis.
        /// </summary>
        [XmlElement("salastada_sertifikaadiga", Namespace = NamespaceConstants.Xtee)]
        string SalastadaSertifikaadiga { get; }

        /// <summary>
        /// Kui päringu välja päises oli element salastada ja päringulogi salastamine õnnestus, siis vastuse päisesse
        /// lisatakse tühi element salastatud.
        /// </summary>
        [XmlElement("salastatud", Namespace = NamespaceConstants.Xtee)]
        string Salastatud { get; }

        /// <summary>
        /// Kui päringu päises oli element salastada_sertifikaadiga ja päringulogi salastamine õnnestus, siis vastuse päisesesse
        /// lisatakse tühi element salastatud_sertifikaadiga.
        /// </summary>
        [XmlElement("salastatud_sertifikaadiga", Namespace = NamespaceConstants.Xtee)]
        string SalastatudSertifikaadiga { get; }
    }
}