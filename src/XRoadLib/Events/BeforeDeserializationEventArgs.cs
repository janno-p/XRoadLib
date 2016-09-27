using System;
using System.Xml;

namespace XRoadLib.Events
{
    /// <summary>
    /// Allows to customize XML reader before deserialization of the X-Road
    /// message begins.
    /// </summary>
    public class BeforeDeserializationEventArgs : EventArgs
    {
        /// <summary>
        /// Customize deserialization by assigning custom settings of XML reader.
        /// </summary>
        public XmlReaderSettings XmlReaderSettings { get; set; }
    }
}