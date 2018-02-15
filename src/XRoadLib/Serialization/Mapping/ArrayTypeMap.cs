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
        private readonly ISerializer serializer;

        private readonly ITypeMap elementTypeMap;

        public ArrayTypeMap(ISerializer serializer, CollectionDefinition collectionDefinition, ITypeMap elementTypeMap)
            : base(collectionDefinition)
        {
            this.serializer = serializer;
            this.elementTypeMap = elementTypeMap;
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
        {
            if (reader.IsEmptyElement)
                return MoveNextAndReturn(reader, new T[0]);

            var items = new List<T>();

            var arrayContent = (ArrayContentDefiniton)content;

            var parentDepth = arrayContent.MergeContent ? reader.Depth - 1 : reader.Depth;
            var itemDepth = parentDepth + 1;
            var itemName = arrayContent.Item.Content.Name.LocalName;

            if (!arrayContent.MergeContent)
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
                    if (arrayContent.MergeContent)
                        break;

                    if (!arrayContent.Item.AcceptsAnyName)
                        throw new Exception($"Invalid array item name {reader.LocalName}.");
                }

                if (reader.IsNilElement())
                {
                    items.Add(default(T));
                    reader.Read();
                    continue;
                }

                var typeMap = serializer.GetTypeMapFromXsiType(reader) ?? elementTypeMap;

                var value = typeMap.Deserialize(reader, templateNode, arrayContent.Item.Content, message);

                items.Add((T)value);
            }

            return items.ToArray();
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
        {
            var valueArray = (Array)value;

            if (!(content.Particle is RequestDefinition))
                message.Style.WriteExplicitArrayType(writer, elementTypeMap.Definition.Name, valueArray.Length);

            var arrayContent = (ArrayContentDefiniton)content;
            var itemName = arrayContent.Item.Content.Name.LocalName;

            foreach (var valueItem in valueArray)
            {
                writer.WriteStartElement(itemName);

                if (valueItem != null)
                {
                    var typeMap = serializer != null ? serializer.GetTypeMap(valueItem.GetType()) : elementTypeMap;
                    typeMap.Serialize(writer, templateNode, valueItem, arrayContent.Item.Content, message);
                }
                else writer.WriteNilAttribute();

                writer.WriteEndElement();
            }
        }
    }
}