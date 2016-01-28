using System;
using System.Reflection;

namespace XRoadLib.Schema
{
    public class PropertyDefinition : ContentDefinition<PropertyInfo>
    {
        public TypeDefinition Owner { get; }

        public PropertyDefinition(TypeDefinition owner)
        {
            Owner = owner;
        }

        public override string ContainerName => $"{RuntimeInfo.DeclaringType?.FullName}";
        public override Type RuntimeType => RuntimeInfo.PropertyType;

        public override string RuntimeName
        {
            get
            {
                var startIndex = RuntimeInfo.Name.LastIndexOf('.');
                return startIndex >= 0 ? RuntimeInfo.Name.Substring(startIndex + 1) : RuntimeInfo.Name;
            }
        }
    }
}