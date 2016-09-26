using System.IO;
using System.Text;
using System.Xml;
using XRoadLib.Protocols.Headers;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Tests.Serialization.Mapping
{
    public abstract class TypeMapTestBase
    {
        protected static readonly SchemaDefinitionReader schemaDefinitionReader = new SchemaDefinitionReader("");

        protected static object DeserializeValue(ITypeMap typeMap, object value)
        {
            var writer = new StringBuilder();
            writer.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            writer.AppendLine($"<value>{value}</value>");

            using (var textReader = new StringReader(writer.ToString()))
            using (var reader = XmlReader.Create(textReader))
            {
                while (reader.Read() && reader.NodeType != XmlNodeType.Element) { }
                using (var message = Globals.XRoadProtocol20.NewMessage())
                    return typeMap.Deserialize(reader, null, Globals.GetTestDefinition(typeMap.Definition.Type), message);
            }
        }
    }
}