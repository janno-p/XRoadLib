using System;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public class ArrayItemDefinition : IContentDefinition
    {
        public XName Name { get; set; }

        public XName TypeName { get; set; }

        public bool IsNullable { get; set; }

        public bool IsOptional { get; set; }

        public bool MergeContent { get; set; }

        public bool UseXop { get; set; }

        public Type TypeMapType { get; set; }

        public Type RuntimeType { get; set; }
    }
}