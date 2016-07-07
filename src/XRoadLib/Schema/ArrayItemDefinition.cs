using System;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public class ArrayItemDefinition : IContentDefinition
    {
        public XName Name { get; set; }

        public XName TypeName { get; set; }

        public bool AcceptAnyName { get; set; }

        public bool IsNullable { get; set; }

        public bool IsOptional { get; set; }

        public bool UseXop { get; set; }

        public Type TypeMapType { get; set; }

        public Type RuntimeType { get; set; }

        public IContentDefinition WrapperDefinition { get; }

        bool IContentDefinition.MergeContent => false;

        ArrayItemDefinition IContentDefinition.ArrayItemDefinition { get { throw new NotImplementedException(); } }

        public ArrayItemDefinition(IContentDefinition wrapperDefinition)
        {
            WrapperDefinition = wrapperDefinition;
        }
    }
}