using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Tests.Contract.Configuration;

namespace XRoadLib.Tests.Serialization.Mapping
{
    public abstract class TypeMapTestBase
    {
        protected static readonly SchemaDefinitionProvider schemaDefinitionProvider = new SchemaDefinitionProvider(new CustomSchemaExporterXRoad20());

        protected static object DeserializeValue(ITypeMap typeMap, object value)
        {
            var writer = new StringBuilder();
            writer.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            writer.AppendLine($"<value>{value}</value>");

            using (var textReader = new StringReader(writer.ToString()))
            using (var reader = XmlReader.Create(textReader))
            {
                while (reader.Read() && reader.NodeType != XmlNodeType.Element) { }
                using (var message = Globals.ServiceManager20.CreateMessage())
                    return typeMap.Deserialize(reader, null, Globals.GetTestDefinition(typeMap.Definition.Type), message);
            }
        }

        protected static object DeserializeEmptyValue(ITypeMap typeMap)
        {
            var writer = new StringBuilder();
            writer.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            writer.AppendLine("<value />");

            using (var textReader = new StringReader(writer.ToString()))
            using (var reader = XmlReader.Create(textReader))
            {
                while (reader.Read() && reader.NodeType != XmlNodeType.Element) { }
                using (var message = Globals.ServiceManager20.CreateMessage())
                    return typeMap.Deserialize(reader, null, Globals.GetTestDefinition(typeMap.Definition.Type), message);
            }
        }
    }
}