using System;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Extensions.ProtoBuf
{
    /// <summary>
    /// Provides protocol buffers serialization/deserialization interface for X-Road operations.
    /// </summary>
    public class ProtoBufServiceMap : IServiceMap
    {
        public OperationDefinition OperationDefinition
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public RequestValueDefinition RequestValueDefinition
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ResponseValueDefinition ResponseValueDefinition
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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
    }
}