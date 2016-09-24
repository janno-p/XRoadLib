namespace XRoadLib.Schema
{
    /// <summary>
    /// Schema exporter for X-Road message protocol version 2.0.
    /// </summary>
    public class SchemaExporterXRoad20 : SchemaExporterBase
    {
        /// <summary>
        /// Configure response elements of X-Road message protocol version 2.0 messages.
        /// </summary>
        public override void ExportResponseValueDefinition(ResponseValueDefinition responseValueDefinition)
        {
            base.ExportResponseValueDefinition(responseValueDefinition);

            responseValueDefinition.ContainsNonTechnicalFault = true;
        }
    }
}