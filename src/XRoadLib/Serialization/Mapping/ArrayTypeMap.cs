using System;
using System.Collections.Generic;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public interface IArrayTypeMap { }

    public class ArrayTypeMap<T> : TypeMap, IArrayTypeMap
    {
        private readonly ISerializerCache serializerCache;

        private readonly ITypeMap elementTypeMap;

        public ArrayTypeMap(ISerializerCache serializerCache, CollectionDefinition collectionDefinition, ITypeMap elementTypeMap)
            : base(collectionDefinition)
        {
            this.serializerCache = serializerCache;
            this.elementTypeMap = elementTypeMap;
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, IContentDefinition definition, XRoadMessage message)
        {
            if (reader.IsEmptyElement)
                return MoveNextAndReturn(reader, new T[0]);

            var items = new List<T>();

            var parentDepth = definition.MergeContent ? reader.Depth - 1 : reader.Depth;
            var itemDepth = parentDepth + 1;
            var itemName = definition.ArrayItemDefinition.Name.LocalName;

            if (!definition.MergeContent)
                reader.Read();

            while (parentDepth < reader.Depth)
            {
                if (reader.NodeType != XmlNodeType.Element || reader.Depth != itemDepth)
                {
                    reader.Read();
                    continue;
                }

                if (reader.LocalName != itemName)
                {
                    if (definition.MergeContent)
                        break;
                    throw new Exception($"Invalid array item name {reader.LocalName}.");
                }

                if (reader.IsNilElement())
                {
                    items.Add(default(T));
                    reader.Read();
                    continue;
                }

                var typeMap = serializerCache.GetTypeMapFromXsiType(reader) ?? elementTypeMap;

                var value = typeMap.Deserialize(reader, templateNode, definition.ArrayItemDefinition, message);

                items.Add(value == null ? default(T) : (T)value);
            }

            return items.ToArray();
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, IContentDefinition definition, XRoadMessage message)
        {
            var valueArray = (Array)value;

            if (!(definition is RequestValueDefinition))
                message.Protocol.Style.WriteExplicitArrayType(writer, elementTypeMap.Definition.Name, valueArray.Length);

            var arrayItemDefinition = definition.ArrayItemDefinition;
            var itemName = arrayItemDefinition.Name.LocalName;

            foreach (var valueItem in valueArray)
            {
                writer.WriteStartElement(itemName);

                if (valueItem != null)
                {
                    var typeMap = serializerCache.GetTypeMap(valueItem.GetType());
                    typeMap.Serialize(writer, templateNode, valueItem, arrayItemDefinition, message);
                }
                else writer.WriteNilAttribute();

                writer.WriteEndElement();
            }
        }
    }
}