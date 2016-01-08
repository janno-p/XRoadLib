namespace XRoadLib.Description
{
    public sealed class ProducerDefinitionSettings
    {
        public string RequestTypeNameFormat { get; set; }
        public string ResponseTypeNameFormat { get; set; }

        public string RequestMessageNameFormat { get; set; }
        public string ResponseMessageNameFormat { get; set; }

        public string ServiceName { get; set; }
        public string BindingName { get; set; }
        public string PortTypeName { get; set; }

        public string XRoadHeaderName { get; set; }
    }
}