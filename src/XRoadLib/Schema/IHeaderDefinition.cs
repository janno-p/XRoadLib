using System.Collections.Generic;
using System.Xml.Linq;
using XRoadLib.Headers;

namespace XRoadLib.Schema
{
    public interface IHeaderDefinition
    {
        /// <summary>
        /// Names of SOAP header elements required by service description.
        /// </summary>
        ISet<XName> RequiredHeaders { get; }

        /// <summary>
        /// Name of WSDL message used to define SOAP header elements.
        /// </summary>
        string MessageName { get; }

        /// <summary>
        /// Creates new instance of definition specific SOAP header.
        /// </summary>
        /// <returns>Definition specific SOAP header</returns>
        ISoapHeader CreateHeader();

        /// <summary>
        /// Test if given namespace is defined as SOAP header element namespace.
        /// </summary>
        bool IsHeaderNamespace(string namespaceName);
    }
}