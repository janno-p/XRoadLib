namespace XRoadLib.Configuration
{
    public interface IXRoadContractConfiguration
    {
        /// <summary>
        /// Name used for binding element in service description.
        /// </summary>
        string BindingName { get; }

        /// <summary>
        /// Name used for portType element in service description.
        /// </summary>
        string PortTypeName { get; }

        /// <summary>
        /// Name used for service element in service description.
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Name used for port element in service description service element.
        /// </summary>
        string ServicePortName { get; }

        /// <summary>
        /// Name of the message that defines mandatory X-Road service headers for operations.
        /// </summary>
        string StandardHeaderName { get; }

        /// <summary>
        /// Name format used for request type naming.
        /// Should contain exactly one string placeholder.
        /// </summary>
        string RequestTypeNameFormat { get; }

        /// <summary>
        /// Name format used for response type naming.
        /// Should contain exactly one string placeholder.
        /// </summary>
        string ResponseTypeNameFormat { get; }

        /// <summary>
        /// Name format used for request message naming.
        /// Should contain exactly one string placeholder.
        /// </summary>
        string RequestMessageNameFormat { get; }

        /// <summary>
        /// Name format used for response message naming.
        /// Should contain exactly one string placeholder.
        /// </summary>
        string ResponseMessageNameFormat { get; }

        /// <summary>
        /// Minimal version number for operations defined inside attribute target assembly. Used by versioning subsystem.
        /// </summary>
        uint? MinOperationVersion { get; }

        /// <summary>
        /// Maximal version number for operations defined inside attribute target assembly. Used by versioning subsystem.
        /// </summary>
        uint? MaxOperationVersion { get; }

        /// <summary>
        /// Allows to customize appearance of types in service description and serialized messages.
        /// </summary>
        ITypeConfiguration TypeConfiguration { get; }

        /// <summary>
        /// Allows to customize appearance of operations in service description and serialized messages.
        /// </summary>
        IOperationConfiguration OperationConfiguration { get; }
    }
}