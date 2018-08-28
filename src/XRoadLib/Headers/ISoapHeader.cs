using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Styles;

namespace XRoadLib.Headers
{
    public interface ISoapHeader
    {
        /// <summary>
        /// Serializes X-Road message SOAP headers to XML.
        /// </summary>
        void WriteTo(XmlWriter writer, Style style, HeaderDefinition headerDefinition);
    }
}