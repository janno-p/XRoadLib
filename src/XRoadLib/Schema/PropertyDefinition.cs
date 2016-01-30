using System.Reflection;

namespace XRoadLib.Schema
{
    public class PropertyDefinition : ContentDefinition
    {
        public TypeDefinition Owner { get; }

        public PropertyInfo PropertyInfo { get; set; }

        public PropertyDefinition(TypeDefinition owner)
        {
            Owner = owner;
        }

        public override string ContainerName => $"{PropertyInfo.DeclaringType?.FullName}";

        public override string RuntimeName
        {
            get
            {
                var startIndex = PropertyInfo.Name.LastIndexOf('.');
                return startIndex >= 0 ? PropertyInfo.Name.Substring(startIndex + 1) : PropertyInfo.Name;
            }
        }
    }
}