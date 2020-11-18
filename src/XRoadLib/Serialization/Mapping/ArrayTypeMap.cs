using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public override async Task<object> DeserializeAsync(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
        {
            var arrayContent = (ArrayContentDefiniton)content;

            if (reader.IsEmptyElement && !arrayContent.MergeContent)
                return await reader.MoveNextAndReturnAsync(new T[0]).ConfigureAwait(false);

            var items = new List<T>();

            var parentDepth = arrayContent.MergeContent ? reader.Depth - 1 : reader.Depth;
            var itemDepth = parentDepth + 1;
            var itemName = arrayContent.Item.Content.Name;

            if (!arrayContent.MergeContent)
                await reader.ReadAsync().ConfigureAwait(false);

            while (parentDepth < reader.Depth)
            {
                if (reader.NodeType != XmlNodeType.Element || reader.Depth != itemDepth)
                {
                    await reader.ReadAsync().ConfigureAwait(false);
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
                    await reader.ReadAsync().ConfigureAwait(false);
                    continue;
                }

                var typeMap = _serializer.GetTypeMapFromXsiType(reader, arrayContent.Item) ?? _elementTypeMap;

                var value = await typeMap.DeserializeAsync(reader, templateNode, arrayContent.Item.Content, message).ConfigureAwait(false);

                items.Add((T)value);
            }

            return items.ToArray();
        }

        public override async Task SerializeAsync(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
        {
            var valueArray = (Array)value;

            if (!(content.Particle is RequestDefinition) && !content.MergeContent)
                await message.Style.WriteExplicitArrayTypeAsync(writer, _elementTypeMap.Definition.Name, valueArray.Length).ConfigureAwait(false);

            var arrayContent = (ArrayContentDefiniton)content;
            var itemName = arrayContent.Item.Content.Name;

            foreach (var valueItem in valueArray)
            {
                await writer.WriteStartElementAsync(itemName).ConfigureAwait(false);

                if (valueItem != null)
                {
                    var typeMap = _serializer != null ? _serializer.GetTypeMap(valueItem.GetType()) : _elementTypeMap;
                    await typeMap.SerializeAsync(writer, templateNode, valueItem, arrayContent.Item.Content, message).ConfigureAwait(false);
                }
                else await writer.WriteNilAttributeAsync().ConfigureAwait(false);

                await writer.WriteEndElementAsync().ConfigureAwait(false);
            }
        }
    }
}