using System;
using XRoadLib.Protocols.Headers;

namespace XRoadLib.Schema
{
    /// <summary>
    /// Allows to fluently configure headers mandatory elements.
    /// </summary>
    public interface IHeaderDefinitionBuilder<THeader> where THeader : IXRoadHeader
    { }

    /// <summary>
    /// Configuration options of X-Road header.
    /// </summary>
    public class HeaderDefinition
    {
        /// <summary>
        /// Create new instance of header object.
        /// </summary>
        public Func<IXRoadHeader> Initializer { get; private set; }

        /// <summary>
        /// Define custom header type for X-Road messages.
        /// </summary>
        public IHeaderDefinitionBuilder<THeader> Use<THeader>(Func<THeader> initializer) where THeader : IXRoadHeader
        {
            Initializer = () => initializer();

            return new HeaderDefinitionBuilder<THeader>();
        }

        private class HeaderDefinitionBuilder<THeader> : IHeaderDefinitionBuilder<THeader> where THeader : IXRoadHeader
        {

        }
    }
}
