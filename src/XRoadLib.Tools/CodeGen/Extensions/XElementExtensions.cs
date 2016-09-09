using System.Xml.Linq;

namespace XRoadLib.Tools.CodeGen.Extensions
{
    public static class XElementExtensions
    {
        public static string GetName(this XElement element)
        {
            return element.Attribute("name")?.Value;
        }

        public static bool IsNullable(this XElement element)
        {
            return (element.Attribute("nillable")?.AsBoolean()).GetValueOrDefault(false);
        }

        public static bool IsCollection(this XElement element)
        {
            return element.GetMaxOccurs() > 1;
        }

        public static bool IsOptional(this XElement element)
        {
            return element.GetMinOccurs() == 0 && !element.IsCollection();
        }

        public static int GetMinOccurs(this XElement element)
        {
            return (element.Attribute("minOccurs")?.AsInt32()).GetValueOrDefault(1);
        }

        public static int GetMaxOccurs(this XElement element)
        {
            return (element.Attribute("maxOccurs")?.AsInt32()).GetValueOrDefault(1);
        }

        public static bool HasAttribute(this XElement element, string attributeName)
        {
            return element.Attribute(attributeName) != null;
        }
    }
}