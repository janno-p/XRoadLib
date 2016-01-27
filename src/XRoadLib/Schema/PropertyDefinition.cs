using System.Reflection;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public class PropertyDefinition : ContentDefinition<PropertyInfo>
    {
        public TypeDefinition Owner { get; }

        public XName TypeName { get; set; }

        public PropertyDefinition(TypeDefinition owner)
        {
            Owner = owner;
        }
    }
}