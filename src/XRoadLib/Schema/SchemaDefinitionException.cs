using System.Runtime.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Schema;

[Serializable]
public class SchemaDefinitionException : XRoadException
{
    public SchemaDefinitionException(string message)
        : base(ServerFaultCode.InternalError, message)
    { }

    protected SchemaDefinitionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}