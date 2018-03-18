using XRoadLib.Soap;

namespace XRoadLib
{
    public class SchemaDefinitionException : XRoadException
    {
        public SchemaDefinitionException(string message)
            : base(ServerFaultCode.InternalError, message)
        { }
    }
}