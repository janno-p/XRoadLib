using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Header
{
    internal class XRoadHeader : IXRoadHeader
    {
        public string Autentija { get; set; }
        public string AmetnikNimi { get; set; }
        public string Amet { get; set; }
        public string Allasutus { get; set; }
        public string Toimik { get; set; }
        public XRoadServiceName Nimi { get; set; }
        public string Id { get; set; }
        public string Isikukood { get; set; }
        public string Andmekogu { get; set; }
        public string Asutus { get; set; }
        public string Ametnik { get; set; }

        public IDictionary<XmlQualifiedName, string> Unresolved { get; } = new Dictionary<XmlQualifiedName, string>();
    }
}