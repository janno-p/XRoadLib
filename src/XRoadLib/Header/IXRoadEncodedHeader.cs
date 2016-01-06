using System.Xml.Serialization;

namespace XRoadLib.Header
{
    /// <summary>
    /// Vastab X-Road skeemis defineeritud kompleks-tüübile {http://x-tee.riik.ee/xsd/xtee.xsd}hdrstd.
    /// </summary>
    public interface IXRoadEncodedHeader
    {
        /// <summary>
        /// Asutuse DNS-nimi.
        /// </summary>
        [XmlElement("asutus")]
        string Asutus { get; set; }

        /// <summary>
        /// Andmekogu DNS-nimi.
        /// </summary>
        [XmlElement("andmekogu")]
        string Andmekogu { get; set; }

        /// <summary>
        /// Teenuse kasutaja isikukood, millele eelneb kahekohaline maa kood.
        /// Nt. EE37702026518.
        /// </summary>
        [XmlElement("isikukood")]
        string Isikukood { get; set; }

        /// <summary>
        /// Teenuse kasutaja Eesti isikukood.
        /// </summary>
        [XmlElement("ametnik")]
        string Ametnik { get; set; }

        /// <summary>
        /// Teenuse väljakutse nonss (unikaalne identifikaator).
        /// </summary>
        [XmlElement("id")]
        string Id { get; set; }

        /// <summary>
        /// Kutsutava teenuse nimi.
        /// </summary>
        [XmlElement("nimi")]
        string Nimi { get; set; }

        /// <summary>
        /// Teenuse väljakutsega seonduva toimiku number (mittekohustuslik)
        /// </summary>
        [XmlElement("toimik")]
        string Toimik { get; set; }

        #region Päringu päises täiendavalt kasutatavad elemendid

        /// <summary>
        /// Asutuse registrikood, mille nimel teenust kasutatakse (kasutusel juriidilise isiku portaalis).
        /// </summary>
        [XmlElement("allasutus")]
        string Allasutus { get; set; }

        /// <summary>
        /// Teenuse kasutaja ametikoht.
        /// </summary>
        [XmlElement("amet")]
        string Amet { get; set; }

        /// <summary>
        /// Teenuse kasutaja nimi.
        /// </summary>
        [XmlElement("ametniknimi")]
        string AmetnikNimi { get; set; }

        /// <summary>
        /// Teenuse kasutamise asünkroonsus.Kui väärtus on "true", siis sooritab turvaserver päringu asünkroonselt.
        /// </summary>
        [XmlElement("asynkroonne")]
        bool Asünkroonne { get; set; }

        /// <summary>
        /// Teenuse kasutaja autentimise viis.Võimalikud variandid on:
        /// • ID – ID-kaardiga autenditud;
        /// • SERT – muu sertifikaadiga autenditud;
        /// • PANK – panga kaudu autenditud;
        /// • PAROOL – kasutajatunnuse ja parooliga autenditud.
        /// Autentimise viisi järel võib sulgudes olla täpsustus (näiteks panga kaudu autentimisel panga tunnus infosüsteemis).
        /// </summary>
        [XmlElement("autentija")]
        string Autentija { get; set; }

        /// <summary>
        /// Teenuse kasutamise eest makstud summa.
        /// </summary>
        [XmlElement("makstud")]
        string Makstud { get; set; }

        /// <summary>
        /// Kui asutusele on X-tee keskuse poolt antud päringute salastamise õigus ja andmekogu on nõus päringut
        /// salastama, siis selle elemendi olemasolul päringu päises andmekogu turvaserver krüpteerib päringu logi,
        /// kasutades selleks X-tee keskuse salastusvõtit.
        /// </summary>
        [XmlElement("salastada")]
        string Salastada { get; set; }

        /// <summary>
        /// Päringu sooritaja ID-kaardi autentimissertifikaat DER-kujul base64 kodeerituna. Selle elemendi olemasolu
        /// päringu päises väljendab soovi päringu logi salastamiseks asutuse turvaserveris päringu sooritaja ID-kaardi
        /// autentimisvõtmega. Seda välja kasutatakse ainult kodaniku päringute portaalis.
        /// </summary>
        [XmlElement("salastada_sertifikaadiga")]
        string SalastadaSertifikaadiga { get; set; }

        /// <summary>
        /// Kui päringu välja päises oli element salastada ja päringulogi salastamine õnnestus, siis vastuse päisesse
        /// lisatakse tühi element salastatud.
        /// </summary>
        [XmlElement("salastatud")]
        string Salastatud { get; set; }

        /// <summary>
        /// Kui päringu päises oli element salastada_sertifikaadiga ja päringulogi salastamine õnnestus, siis vastuse päisesesse
        /// lisatakse tühi element salastatud_sertifikaadiga.
        /// </summary>
        [XmlElement("salastatud_sertifikaadiga")]
        string SalastatudSertifikaadiga { get; set; }

        #endregion
    }
}