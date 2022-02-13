using System.Text;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Tests.Serialization.Mapping;

public abstract class TypeMapTestBase
{
    protected static readonly SchemaDefinitionProvider SchemaDefinitionProvider = new(new DefaultSchemaExporter("urn:some-namespace", typeof(Contract.Class1).Assembly));

    protected static Task<object> DeserializeValueAsync(ITypeMap typeMap, object value)
    {
        var writer = new StringBuilder();
        writer.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
        writer.AppendLine($"<value>{value}</value>");

        using var textReader = new StringReader(writer.ToString());
        using var reader = XmlReader.Create(textReader, new XmlReaderSettings { Async = true });

        while (reader.Read() && reader.NodeType != XmlNodeType.Element)
        {
            // Do nothing
        }

        using var message = Globals.ServiceManager.CreateMessage();
        return typeMap.DeserializeAsync(reader, XRoadXmlTemplate.EmptyNode, Globals.GetTestDefinition(typeMap.Definition.Type), message);
    }

    protected static async Task<object> DeserializeEmptyValueAsync(ITypeMap typeMap)
    {
        var writer = new StringBuilder();
        writer.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
        writer.AppendLine("<value />");

        using var textReader = new StringReader(writer.ToString());
        using var reader = XmlReader.Create(textReader, new XmlReaderSettings { Async = true });

        while (await reader.ReadAsync() && reader.NodeType != XmlNodeType.Element)
        {
            // Do nothing
        }

        using var message = Globals.ServiceManager.CreateMessage();
        return await typeMap.DeserializeAsync(reader, XRoadXmlTemplate.EmptyNode, Globals.GetTestDefinition(typeMap.Definition.Type), message);
    }
}