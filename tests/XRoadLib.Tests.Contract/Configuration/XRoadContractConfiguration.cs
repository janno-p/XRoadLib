using XRoadLib.Configuration;

namespace XRoadLib.Tests.Contract.Configuration
{
    public class XRoadContractConfiguration : XRoadContractConfigurationBase
    {
        public override uint? MinOperationVersion => 1u;
        public override uint? MaxOperationVersion => 2u;
        public override string StandardHeaderName => "stdhdr";
        public override string PortTypeName => "TestProducerPortType";
        public override string BindingName => "TestBinding";
        public override string ServicePortName => "TestPort";
        public override string ServiceName => "TestService";
        public override string RequestTypeNameFormat => "{0}Request";
        public override string ResponseTypeNameFormat => "{0}Response";
        public override string RequestMessageNameFormat => "{0}";
        public override string ResponseMessageNameFormat => "{0}Response";
        public override XRoadContentLayoutMode OperationContentLayoutMode => XRoadContentLayoutMode.Strict;
    }
}