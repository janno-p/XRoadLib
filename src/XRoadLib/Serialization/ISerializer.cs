using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Serialization
{
    /// <summary>
    /// Handles runtime operations for schema definition.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Common DTO version of serialized types.
        /// </summary>
        uint? Version { get; }

        /// <summary>
        /// Get serialization details for operation defined in producer namespace.
        /// </summary>
        /// <param name="operationName">Operation local name in producer namespace.</param>
        /// <returns>Service serialization implementation.</returns>
        IServiceMap GetServiceMap(string operationName);

        /// <summary>
        /// Get serialization details for operation by its fully qualifid name.
        /// </summary>
        /// <param name="qualifiedName">Qualified name of operation.</param>
        /// <returns>Service serialization implementation.</returns>
        IServiceMap GetServiceMap(XName qualifiedName);

        /// <summary>
        /// Get type serialization info using xsi:type attribute of current element
        /// in XmlReader instance.
        /// </summary>
        /// <param name="reader">XmlReader instance which parses current XML document.</param>
        /// <returns>Type serialization details.</returns>
        ITypeMap GetTypeMapFromXsiType(XmlReader reader);

        /// <summary>
        /// Get type serialization info using qualified type name.
        /// </summary>
        /// <param name="qualifiedName">Qualified name of serializable type.</param>
        /// <param name="isArray">Request array serialization info.</param>
        /// <returns>Type serialization details.</returns>
        ITypeMap GetTypeMap(XName qualifiedName, bool isArray);

        /// <summary>
        /// Get type serialization info for specified runtime type.
        /// Handles partial type maps to build mutual references between runtime types.
        /// </summary>
        /// <param name="runtimeType">Runtime type which implementation is requested.</param>
        /// <param name="partialTypeMaps">Known partial type implementations.</param>
        /// <returns>Type serialization details.</returns>
        ITypeMap GetTypeMap(Type runtimeType, IDictionary<Type, ITypeMap> partialTypeMaps = null);

        /// <summary>
        /// Retrieves XML type name for specified runtime type.
        /// </summary>
        /// <param name="type">Runtime type.</param>
        /// <returns>XML type name.</returns>
        XName GetXmlTypeName(Type type);
    }
}