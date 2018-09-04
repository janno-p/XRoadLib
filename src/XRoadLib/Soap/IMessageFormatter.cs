using System.Xml;

namespace XRoadLib.Soap
{
    public interface IMessageFormatter
    {
        void MoveToEnvelope(XmlReader reader);
        bool TryMoveToHeader(XmlReader reader);
        bool TryMoveToBody(XmlReader reader);
    }
}