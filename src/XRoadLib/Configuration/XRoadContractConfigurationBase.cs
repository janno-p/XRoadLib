namespace XRoadLib.Configuration
{
    public class XRoadContractConfigurationBase : IXRoadContractConfiguration
    {
        public virtual string BindingName { get; }
        public virtual string PortTypeName { get; }
        public virtual string ServiceName { get; }
        public virtual string ServicePortName { get; }
        public virtual string StandardHeaderName { get; }
        public virtual string RequestTypeNameFormat { get; }
        public virtual string ResponseTypeNameFormat { get; }
        public virtual string RequestMessageNameFormat { get; }
        public virtual string ResponseMessageNameFormat { get; }
        public virtual XRoadContentLayoutMode OperationContentLayoutMode { get; }
        public virtual IParameterNameProvider ParameterNameProvider { get; }
        public virtual uint? MinOperationVersion { get; }
        public virtual uint? MaxOperationVersion { get; }
        public virtual ITypeConfigurationProvider TypeConfigurationProvider { get; }
    }
}