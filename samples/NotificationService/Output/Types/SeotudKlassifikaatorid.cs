using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class SeotudKlassifikaatorid : IXRoadXmlSerializable
    {
        public Option<long> KlassifikaatoriObjektID { get; set; }
        public Option<long?> SeoseLiikKL { get; set; }
        public Option<long> SeotudKlassifikaatoriObjektID { get; set; }

        public class SeotudKLVaartusedType : IXRoadXmlSerializable
        {
            public IList<SeotudKLVaartused> item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public Option<SeotudKLVaartusedType> SeotudKLVaartused { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}