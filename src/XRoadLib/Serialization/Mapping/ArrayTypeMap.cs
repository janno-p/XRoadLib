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
        private readonly ISerializer _serializer;

        private readonly ITypeMap _elementTypeMap;

        public ArrayTypeMap(ISerializer serializer, CollectionDefinition collectionDefinition, ITypeMap elementTypeMap)
            : base(collectionDefinition)
        {
            _serializer = serializer;
            _elementTypeMap = elementTypeMap;
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
        {
            var arrayContent = (ArrayContentDefiniton)content;

            if (reader.IsEmptyElement && !arrayContent.MergeContent)
                return reader.MoveNextAndReturn(new T[0]);

            var items = new List<T>();

            var parentDepth = arrayContent.MergeContent ? reader.Depth - 1 : reader.Depth;
            var itemDepth = parentDepth + 1;
            var itemName = arrayContent.Item.Content.Name;

            if (!arrayContent.MergeContent)
                reader.Read();

            while (parentDepth < reader.Depth)
            {
                if (reader.NodeType != XmlNodeType.Element || reader.Depth != itemDepth)
                {
                    reader.Read();
                    continue;
                }

                if (reader.GetXName() != itemName)
                {
                    if (arrayContent.MergeContent)
                        break;

                    if (!arrayContent.Item.AcceptsAnyName)
                    {
                        var readerName = reader.GetXName();
                        throw new UnexpectedElementException($"Invalid array item name `{readerName}`.", Definition, arrayContent.Item, readerName);
                    }
                }

                if (reader.IsNilElement())
                {
                    items.Add(default);
                    reader.Read();
                    continue;
                }

                var typeMap = _serializer.GetTypeMapFromXsiType(reader, arrayContent.Item) ?? _elementTypeMap;

                var value = typeMap.Deserialize(reader, templateNode, arrayContent.Item.Content, message);

                items.Add((T)value);
            }

            return items.ToArray();
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
        {
            var valueArray = (Array)value;

            if (!(content.Particle is RequestDefinition) && !content.MergeContent)
                message.Style.WriteExplicitArrayType(writer, _elementTypeMap.Definition.Name, valueArray.Length);

            var arrayContent = (ArrayContentDefiniton)content;
            var itemName = arrayContent.Item.Content.Name;

            foreach (var valueItem in valueArray)
            {
                writer.WriteStartElement(itemName);

                if (valueItem != null)
                {
                    var typeMap = _serializer != null ? _serializer.GetTypeMap(valueItem.GetType()) : _elementTypeMap;
                    typeMap.Serialize(writer, templateNode, valueItem, arrayContent.Item.Content, message);
                }
                else writer.WriteNilAttribute();

                writer.WriteEndElement();
            }
        }
    }
}