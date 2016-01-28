using System.Xml.Linq;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Schema
{
    public class ArrayItemDefinition
    {
        public XName Name { get; set; }

        public bool IsNullable { get; set; }

        public bool IsOptional { get; set; }

        public bool UseXop { get; set; }

        public ITypeMap TypeMap { get; set; }
    }
}