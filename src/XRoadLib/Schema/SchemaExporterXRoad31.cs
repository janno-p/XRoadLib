namespace XRoadLib.Schema
{
    /// <summary>
    /// Schema exporter for X-Road message protocol version 3.1.
    /// </summary>
    public class SchemaExporterXRoad31 : SchemaExporterBase
    {
        /// <summary>
        /// Configure response elements of X-Road message protocol version 3.1 messages.
        /// </summary>
        public override void ExportResponseValueDefinition(ResponseValueDefinition responseValueDefinition)
        {
            base.ExportResponseValueDefinition(responseValueDefinition);

            responseValueDefinition.ContainsNonTechnicalFault = true;
        }
    }
}