using System;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public class TypeDefinition
    {
        public XName Name { get; set; }

        public Type RuntimeType { get; set; }
    }
}