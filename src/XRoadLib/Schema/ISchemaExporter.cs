namespace XRoadLib.Schema
{
    /// <summary>
    /// Provides configuration hooks for customizing contract serialization
    /// and definitions.
    /// </summary>
    public interface ISchemaExporter
    {
        /// <summary>
        /// Configuration hook for overriding default operation settings.
        /// </summary>
        void ExportOperationDefinition(OperationDefinition operationDefinition);

        /// <summary>
        /// Configuration hook for overriding default property settings.
        /// </summary>
        void ExportPropertyDefinition(PropertyDefinition propertyDefinition);

        /// <summary>
        /// Configuration hook for overriding default type settings.
        /// </summary>
        void ExportTypeDefinition(TypeDefinition typeDefinition);

        /// <summary>
        /// Configuration hook for overriding default response element settings.
        /// </summary>
        void ExportResponseValueDefinition(ResponseValueDefinition responseValueDefinition);

        /// <summary>
        /// Configuration hook for overriding default request element settings.
        /// </summary>
        void ExportRequestValueDefinition(RequestValueDefinition requestValueDefinition);

        /// <summary>
        /// Configuration hook for overriding default non-technical fault settings.
        /// </summary>
        void ExportFaultDefinition(FaultDefinition faultDefinition);

        /// <summary>
        /// Provide custom schema locations.
        /// </summary>
        string ExportSchemaLocation(string namespaceName);
    }
}