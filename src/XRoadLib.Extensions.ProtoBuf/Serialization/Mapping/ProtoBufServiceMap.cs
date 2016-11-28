using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using System.Xml.Linq;
using Google.Protobuf;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Extensions.ProtoBuf.Serialization.Mapping
{
    /// <summary>
    /// Provides protocol buffers serialization/deserialization interface for X-Road operations.
    /// </summary>
    public class ProtoBufServiceMap : IServiceMap
    {
        private delegate object ReadValueMethod(Stream stream);
        private delegate void WriteValueMethod(Stream stream, object value);

        private readonly ITypeMap typeMap;
        private readonly ReadValueMethod readRequestMethod;
        private readonly ReadValueMethod readResponseMethod;
        private readonly WriteValueMethod writeRequestMethod;
        private readonly WriteValueMethod writeResponseMethod;

        /// <summary>
        /// Configuration settings of the operation that the ServiceMap implements.
        /// </summary>
        public OperationDefinition OperationDefinition { get; }

        /// <summary>
        /// Request element specification of the X-Road operation.
        /// </summary>
        public RequestValueDefinition RequestValueDefinition { get; }

        /// <summary>
        /// Response element specification of the X-Road operation.
        /// </summary>
        public ResponseValueDefinition ResponseValueDefinition { get; }

        public ProtoBufServiceMap(ISerializerCache serializerCache, OperationDefinition operationDefinition, RequestValueDefinition requestValueDefinition, ResponseValueDefinition responseValueDefinition, ITypeMap inputTypeMap, ITypeMap outputTypeMap)
        {
            OperationDefinition = operationDefinition;
            RequestValueDefinition = requestValueDefinition;
            ResponseValueDefinition = responseValueDefinition;

            typeMap = ((IContentTypeMap)serializerCache.GetTypeMap(XName.Get("base64Binary", NamespaceConstants.XSD), false)).GetOptimizedContentTypeMap();

            readRequestMethod = BuildReadValueMethod(RequestValueDefinition);
            readResponseMethod = BuildReadValueMethod(ResponseValueDefinition);
            writeRequestMethod = BuildWriteValueMethod(RequestValueDefinition);
            writeResponseMethod = BuildWriteValueMethod(ResponseValueDefinition);
        }

        public object DeserializeRequest(XmlReader reader, XRoadMessage message)
        {
            var requestName = RequestValueDefinition.RequestElementName;

            if (!RequestValueDefinition.MergeContent && !reader.MoveToElement(3, requestName))
                throw XRoadException.InvalidQuery($"Päringus puudub X-tee `{requestName}` element.");

            if (RequestValueDefinition.ParameterInfo != null)
                return DeserializeValue(reader, RequestValueDefinition, message, readRequestMethod);

            return null;
        }

        public object DeserializeResponse(XmlReader reader, XRoadMessage message)
        {
            var responseName = ResponseValueDefinition.ResponseElementName;

            var hasResponseElement = reader.MoveToElement(3);

            if (hasResponseElement && !ResponseValueDefinition.ContainsNonTechnicalFault && reader.LocalName == ResponseValueDefinition.FaultName)
                return reader.ReadXRoadFault(4);

            if (!hasResponseElement || reader.LocalName != responseName)
                throw XRoadException.InvalidQuery($"Expected payload element `{responseName}` in SOAP message, but `{reader.LocalName}` was found instead.");

            return DeserializeValue(reader, ResponseValueDefinition, message, readResponseMethod);
        }

        public void SerializeRequest(XmlWriter writer, object value, XRoadMessage message, string requestNamespace = null)
        {
            var ns = string.IsNullOrEmpty(requestNamespace) ? OperationDefinition.Name.NamespaceName : requestNamespace;
            var addPrefix = writer.LookupPrefix(ns) == null;

            if (addPrefix) writer.WriteStartElement(PrefixConstants.TARGET, OperationDefinition.Name.LocalName, ns);
            else writer.WriteStartElement(OperationDefinition.Name.LocalName, ns);

            if (!RequestValueDefinition.MergeContent)
                writer.WriteStartElement(RequestValueDefinition.RequestElementName);

            if (RequestValueDefinition.ParameterInfo != null)
                SerializeValue(writer, value, message, RequestValueDefinition, writeRequestMethod);

            if (!RequestValueDefinition.MergeContent)
                writer.WriteEndElement();

            writer.WriteEndElement();
        }

        public void SerializeResponse(XmlWriter writer, object value, XRoadMessage message, XmlReader requestReader, ICustomSerialization customSerialization = null)
        {
            var containsRequest = requestReader.MoveToElement(2, OperationDefinition.Name.LocalName, OperationDefinition.Name.NamespaceName);

            if (containsRequest)
                writer.WriteStartElement(requestReader.Prefix, $"{OperationDefinition.Name.LocalName}Response", OperationDefinition.Name.NamespaceName);
            else writer.WriteStartElement($"{OperationDefinition.Name.LocalName}Response", OperationDefinition.Name.NamespaceName);

            var fault = value as IXRoadFault;
            var namespaceInContext = requestReader.NamespaceURI;

            if (!ResponseValueDefinition.ContainsNonTechnicalFault && fault != null)
            {
                writer.WriteStartElement(ResponseValueDefinition.FaultName);
                SerializeFault(writer, fault, message.Protocol);
                writer.WriteEndElement();
            }
            else
            {
                if (Equals(namespaceInContext, ""))
                    writer.WriteStartElement(ResponseValueDefinition.ResponseElementName);
                else writer.WriteStartElement(ResponseValueDefinition.ResponseElementName, "");

                SerializeValue(writer, value, message, ResponseValueDefinition, writeResponseMethod);

                writer.WriteEndElement();

                customSerialization?.OnContentComplete(writer);
            }

            writer.WriteEndElement();
        }

        private object DeserializeValue(XmlReader reader, IContentDefinition contentDefinition, XRoadMessage message, ReadValueMethod readValueMethod)
        {
            if (reader.IsNilElement())
            {
                reader.ReadToEndElement();
                return null;
            }

            string typeAttribute;
            if (typeMap.Definition.IsAnonymous && !(typeMap is IArrayTypeMap) && (typeAttribute = reader.GetAttribute("type", NamespaceConstants.XSI)) != null)
                throw XRoadException.InvalidQuery($"Expected anonymous type, but `{typeAttribute}` was given.");

            var stream = (Stream)typeMap.Deserialize(reader, null, contentDefinition, message);
            stream.Position = 0;

            return readValueMethod(stream);
        }

        private void SerializeValue(XmlWriter writer, object value, XRoadMessage message, IContentDefinition contentDefinition, WriteValueMethod writeValueMethod)
        {
            if (value == null)
            {
                writer.WriteNilAttribute();
                return;
            }

            var stream = new MemoryStream();
            message.AllAttachments.Add(new XRoadAttachment(stream));

            writeValueMethod(stream, value);

            typeMap.Serialize(writer, null, stream, contentDefinition, message);
        }

        private static void SerializeFault(XmlWriter writer, IXRoadFault fault, IXRoadProtocol protocol)
        {
            writer.WriteStartElement("faultCode");
            protocol.Style.WriteExplicitType(writer, XName.Get("string", NamespaceConstants.XSD));
            writer.WriteValue(fault.FaultCode);
            writer.WriteEndElement();

            writer.WriteStartElement("faultString");
            protocol.Style.WriteExplicitType(writer, XName.Get("string", NamespaceConstants.XSD));
            writer.WriteValue(fault.FaultString);
            writer.WriteEndElement();
        }

        private static ReadValueMethod BuildReadValueMethod(IContentDefinition definition)
        {
            var type = definition.RuntimeType;
            if (type == null)
                return o => null;

            var parserProperty = type.GetProperty("Parser", BindingFlags.Public | BindingFlags.Static);
            var parserType = parserProperty.PropertyType;
            var parseMethod = parserType.GetMethod("ParseFrom", new[] { typeof(Stream) });

            var dynamicRead = new DynamicMethod("DynamicRead", typeof(object), new[] { typeof(Stream) }, type, true);
            var generator = dynamicRead.GetILGenerator();

            generator.Emit(OpCodes.Call, parserProperty.GetGetMethod());
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Callvirt, parseMethod);
            generator.Emit(OpCodes.Ret);

            return (ReadValueMethod)dynamicRead.CreateDelegate(typeof(ReadValueMethod));
        }

        private static WriteValueMethod BuildWriteValueMethod(IContentDefinition definition)
        {
            var type = definition.RuntimeType;
            if (type == null)
                return (o, v) => { };

            var method = typeof(MessageExtensions).GetMethod("WriteTo", BindingFlags.Public | BindingFlags.Static);

            var dynamicWrite = new DynamicMethod("DynamicWrite", typeof(void), new[] { typeof(Stream), typeof(object) }, type, true);
            var generator = dynamicWrite.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Castclass, typeof(IMessage));

            generator.Emit(OpCodes.Ldarg_0);

            generator.Emit(OpCodes.Call, method);
            generator.Emit(OpCodes.Ret);

            return (WriteValueMethod)dynamicWrite.CreateDelegate(typeof(WriteValueMethod));
        }
    }
}