namespace XRoadLib.Schema
{
    /// <summary>
    /// Configuration options for customizing non-technical faults.
    /// </summary>
    public class FaultDefinition : Definition
    {
        /// <summary>
        /// Initializes empty fault definition object.
        /// </summary>
        public FaultDefinition()
        {
            Documentation = new DocumentationDefinition();
        }
    }
}