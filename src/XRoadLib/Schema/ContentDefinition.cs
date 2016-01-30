using System;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public abstract class ContentDefinition : Definition, IContentDefinition
    {
        public bool IsNullable { get; set; }

        public bool IsOptional { get; set; }

        public bool UseXop { get; set; }

        public ArrayItemDefinition ArrayItemDefinition { get; set; }

        public int Order { get; set; }

        public XName TypeName { get; set; }

        public Type RuntimeType { get; set; }

        public abstract string ContainerName { get; }

        public abstract string RuntimeName { get; }
    }
}