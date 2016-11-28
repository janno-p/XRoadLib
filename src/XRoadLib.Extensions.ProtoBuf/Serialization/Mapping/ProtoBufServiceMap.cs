using System;
using System.Xml;
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

        public object DeserializeRequest(XmlReader reader, XRoadMessage message)
        {
            throw new NotImplementedException();
        }

        public object DeserializeResponse(XmlReader reader, XRoadMessage message)
        {
            throw new NotImplementedException();
        }

        public void SerializeRequest(XmlWriter writer, object value, XRoadMessage message, string requestNamespace = null)
        {
            throw new NotImplementedException();
        }

        public void SerializeResponse(XmlWriter writer, object value, XRoadMessage message, XmlReader requestReader, ICustomSerialization customSerialization = null)
        {
            throw new NotImplementedException();
        }

        public ProtoBufServiceMap(ISerializerCache serializerCache, OperationDefinition operationDefinition, RequestValueDefinition requestValueDefinition, ResponseValueDefinition responseValueDefinition, ITypeMap inputTypeMap, ITypeMap outputTypeMap)
        {
            OperationDefinition = operationDefinition;
            RequestValueDefinition = requestValueDefinition;
            ResponseValueDefinition = responseValueDefinition;
        }
    }
}