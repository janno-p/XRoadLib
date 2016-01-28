using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class ArrayTypeMap<T> : TypeMap<T[]>
    {
        private readonly ISerializerCache serializerCache;

        private readonly XName elementQualifiedName;
        private readonly Type elementType = typeof(T);

        public override bool IsSimpleType => false;

        public override XName QualifiedName { get { throw new NotImplementedException(); } }

        public ArrayTypeMap(ISerializerCache serializerCache, XName elementQualifiedName = null)
        {
            this.serializerCache = serializerCache;
            this.elementQualifiedName = elementQualifiedName ?? serializerCache.GetXmlTypeName(typeof(T));
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
        {
            if (reader.IsEmptyElement)
                return new T[0];

            var items = new List<T>();

            var depth = reader.Depth;
            while (reader.Read() && depth < reader.Depth)
            {
                if (reader.NodeType != XmlNodeType.Element || reader.IsNilElement())
                    continue;

                var typeMap = serializerCache.GetTypeMapFromXsiType(reader) ?? serializerCache.GetTypeMap(elementType);

                var value = typeMap.Deserialize(reader, templateNode, context);
                if (value != null)
                    items.Add((T)value);
            }

            return items.ToArray();
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, SerializationContext context)
        {
            var valueArray = (Array)value;

            context.Protocol.Style.WriteExplicitArrayType(writer, elementQualifiedName, valueArray.Length);

            foreach (var element in valueArray)
            {
                writer.WriteStartElement("item");

                if (element != null)
                {
                    var typeMap = serializerCache.GetTypeMap(element.GetType());
                    typeMap.Serialize(writer, templateNode, element, elementType, context);
                }
                else writer.WriteNilAttribute();

                writer.WriteEndElement();
            }
        }
    }
}