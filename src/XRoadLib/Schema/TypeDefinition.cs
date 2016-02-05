using System;
using System.Collections.Generic;
using System.Linq;
using XRoadLib.Extensions;

namespace XRoadLib.Schema
{
    public class TypeDefinition : Definition
    {
        public Type Type { get; }

        public bool CanHoldNullValues { get; set; }

        public bool IsAbstract { get; set; }

        public bool IsAnonymous { get; set; }

        public bool IsSimpleType { get; set; }

        public Type TypeMapType { get; set; }

        public bool HasStrictContentOrder { get; set; }

        public IComparer<PropertyDefinition> ContentComparer { get; set; }

        public bool IsInheritable => !IsAnonymous && !IsSimpleType;

        public TypeDefinition(Type type)
        {
            Documentation = type.GetXRoadTitles().Where(title => !string.IsNullOrWhiteSpace(title.Item2)).ToArray();
            Type = type;
        }
    }
}