using System.IO;
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
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
                writer.WriteLine($"<value>{value}</value>");
                writer.Flush();

                stream.Position = 0;
                using (var reader = XmlReader.Create(stream))
                {
                    while (reader.Read() && reader.NodeType != XmlNodeType.Element)
                    { }

                    using (var message = new XRoadMessage(Globals.XRoadProtocol20, new XRoadHeader20()))
                        return typeMap.Deserialize(reader, null, Globals.GetTestDefinition(typeMap.Definition.Type), message);
                }
            }
        }
    }
}