using System;
using System.Threading.Tasks;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Styles;
using XRoadLib.Wsdl;

namespace XRoadLib
{
    /// <summary>
    /// Handles service oriented tasks based on specific schema definition.
    /// </summary>
    public interface IServiceManager
    {
        /// <summary>
        /// User defined name for service manager in case of multiple definitions.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Operation binding style of Xml messages.
        /// </summary>
        Style Style { get; }

        /// <summary>
        /// Test if given namespace is defined as SOAP header element namespace.
        /// </summary>
        bool IsHeaderNamespace(string namespaceName);

        /// <summary>
        /// Check if envelope defines given protocol schema.
        /// </summary>
        Task<bool> IsDefinedByEnvelopeAsync(XmlReader reader);

        /// <summary>
        /// Main namespace which defines current producer operations and types.
        /// </summary>
        string ProducerNamespace { get; }

        /// <summary>
        /// Header definition of the protocol.
        /// </summary>
        IHeaderDefinition HeaderDefinition { get; }

        /// <summary>
        /// Protocol specification.
        /// </summary>
        ProtocolDefinition ProtocolDefinition { get; }

        /// <summary>
        /// Initializes new XRoadMessage instance, suitable for current manager definition.
        /// </summary>
        /// <returns>Created XRoadMessage instance</returns>
        XRoadMessage CreateMessage();

        /// <summary>
        /// Get serializer instance of schema definition runtime serialization.
        /// </summary>
        /// <param name="version">Global DTO version of wanted serialzier.</param>
        /// <returns>Runtime type serializer.</returns>
        ISerializer GetSerializer(uint? version = null);

        /// <summary>
        /// Generates service description based on current schema definition.
        /// </summary>
        /// <param name="operationFilter">Allows to filter out unwanted operations which should not appear in service description.</param>
        /// <param name="version">Global DTO version of wanted service description.</param>
        /// <returns>Service description instance of current schema definition.</returns>
        ServiceDescription CreateServiceDescription(Func<OperationDefinition, bool> operationFilter = null, uint? version = null);
    }
}