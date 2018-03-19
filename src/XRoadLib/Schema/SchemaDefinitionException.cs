using XRoadLib.Soap;

namespace XRoadLib.Schema
{
    public class SchemaDefinitionException : XRoadException
    {
        public SchemaDefinitionException(string message)
            : base(ServerFaultCode.InternalError, message)
        { }
    }
}