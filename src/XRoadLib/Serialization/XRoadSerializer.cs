using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization
{
    public class XRoadSerializer
    {
        private readonly ISerializerCache serializerCache;

        public XRoadSerializer(ISerializerCache serializerCache)
        {
            this.serializerCache = serializerCache;
        }

        public void SerializeElement(XmlWriter writer, string elementName, object value, SerializationContext context, ITypeMap typeMap = null)
        {
            writer.WriteStartElement(elementName);
            Serialize(writer, value, typeMap, context);
            writer.WriteEndElement();
        }

        public IDictionary<string, object> Deserialize(XmlReader reader, string rootName, SerializationContext context, IDictionary<string, Type> parameterTypeMap = null)
        {
            var elements = new Dictionary<string, object>();

            if (!reader.ReadToElement(rootName))
                return elements;

            foreach (var entry in DeserializeElement(reader, context, parameterTypeMap ?? context.XmlTemplate?.ParameterTypes))
                elements.Add(entry.Key, entry.Value);

            if (elements.Count == 0)
                elements.Add(rootName, null);

            if (context.XmlTemplate != null)
                foreach (var templateNode in context.XmlTemplate.ParameterNodes.Where(n => n.IsRequired && (!elements.ContainsKey(n.Name) || elements[n.Name] == null)))
                    throw XRoadException.TeenuseKohustuslikParameeterPuudub(templateNode.Name);

            return elements;
        }

        private void Serialize(XmlWriter writer, object value, ITypeMap typeMap, SerializationContext context)
        {
            if (value == null)
            {
                writer.WriteNilAttribute();
                return;
            }

            var validatorNode = context.XmlTemplate?.ResponseNode;

            var valueType = value is Exception ? typeof(Exception) : value.GetType();

            typeMap = typeMap ?? serializerCache.GetTypeMap(valueType, context.DtoVersion);

            typeMap.Serialize(writer, validatorNode, value, typeMap.RuntimeType, context);
        }

        private IEnumerable<KeyValuePair<string, object>> DeserializeElement(XmlReader reader, SerializationContext context, IDictionary<string, Type> parameterTypeMap)
        {
            Type elementType = null;
            if (parameterTypeMap != null)
                parameterTypeMap.TryGetValue(reader.LocalName, out elementType);

            if (elementType == null && serializerCache.GetTypeMapFromXsiType(reader, context.DtoVersion, true) == null)
            {
                var depth = reader.Depth;
                while (reader.Read() && depth < reader.Depth)
                    if (reader.NodeType == XmlNodeType.Element)
                        yield return DeserializeParameter(reader, context, parameterTypeMap);
            }
            else yield return DeserializeParameter(reader, context, parameterTypeMap);
        }

        private KeyValuePair<string, object> DeserializeParameter(XmlReader reader, SerializationContext context, IDictionary<string, Type> parameterTypeMap)
        {
            var parameterName = reader.LocalName;

            var templateNode = context.XmlTemplate != null ? context.XmlTemplate.GetParameterNode(parameterName) : XRoadXmlTemplate.EmptyNode;
            if (templateNode == null)
                throw XRoadException.UnknownParameter(parameterName);

            var deserializedParameter = DeserializeParameterContent(reader, templateNode, GetParameterType(parameterName, parameterTypeMap), context);

            return new KeyValuePair<string, object>(parameterName, deserializedParameter);
        }

        private static Type GetParameterType(string parameterName, IDictionary<string, Type> parameterTypeMap)
        {
            return parameterTypeMap?[parameterName];
        }

        private object DeserializeParameterContent(XmlReader reader, IXmlTemplateNode templateNode, Type parameterType, SerializationContext context)
        {
            if (templateNode == null || reader.IsNilElement())
            {
                reader.ReadToEndElement();
                return null;
            }

            var typeMap = serializerCache.GetTypeMapFromXsiType(reader, context.DtoVersion) ?? serializerCache.GetTypeMap(parameterType, context.DtoVersion);

            return typeMap.Deserialize(reader, templateNode, context);
        }
    }
}