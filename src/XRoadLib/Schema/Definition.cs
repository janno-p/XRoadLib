using System;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public abstract class Definition
    {
        public XName Name { get; set; }

        public DefinitionState State { get; set; }

        public DocumentationDefinition Documentation { get; set; }

        public Tuple<XName, string>[] CustomAttributes { get; set; }

        internal static Type NormalizeType(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }
    }
}