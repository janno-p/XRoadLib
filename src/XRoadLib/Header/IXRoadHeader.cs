using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Header
{
    public interface IXRoadHeader
    {
        string Autentija { get; set; }
        string AmetnikNimi { get; set; }
        string Amet { get; set; }
        string Allasutus { get; set; }
        string Toimik { get; set; }
        XRoadServiceName Nimi { get; set; }
        string Id { get; set; }
        string Isikukood { get; set; }
        string Andmekogu { get; set; }
        string Asutus { get; set; }
        string Ametnik { get; set; }

        IDictionary<XmlQualifiedName, string> Unresolved { get; }
    }
}