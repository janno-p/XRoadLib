namespace XRoadLib.Schema
{
    public class OperationDefinition : Definition
    {
        public OperationTypeDefinition OperationTypeDefinition { get; }

        public uint Version { get; set; }
        public bool IsAbstract { get; set; }

        public string InputMessageName { get; set; }
        public BinaryMode InputBinaryMode { get; set; }
        public bool ProhibitRequestPartInResponse { get; set; }

        public string OutputMessageName { get; set; }
        public BinaryMode OutputBinaryMode { get; set; }
        public bool HideXRoadFaultDefinition { get; set; }

        public OperationDefinition(OperationTypeDefinition operationTypeDefinition)
        {
            OperationTypeDefinition = operationTypeDefinition;
        }
    }
}