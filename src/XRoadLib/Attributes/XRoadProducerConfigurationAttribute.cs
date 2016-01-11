using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class XRoadProducerConfigurationAttribute : Attribute, IXRoadProtocolAppliable
    {
        private uint? minOperationVersion;
        private uint? maxOperationVersion;

        private XRoadProtocol? appliesTo;

        /// <summary>
        /// Minimal version number for operations defined inside
        /// attribute target assembly. Used by versioning subsystem.
        /// </summary>
        public uint MinOperationVersion
        {
            get { return minOperationVersion.GetValueOrDefault(1u); }
            set { minOperationVersion = value; }
        }

        public bool HasMinOperationVersion => minOperationVersion.HasValue;

        /// <summary>
        /// Maximal version number for operations defined inside
        /// attribute target assembly. Used by versioning subsystem.
        /// </summary>
        public uint MaxOperationVersion
        {
            get { return maxOperationVersion.GetValueOrDefault(1u); }
            set { maxOperationVersion = value; }
        }

        public bool HasMaxOperationVersion => maxOperationVersion.HasValue;

        /// <summary>
        /// Name of the message that defines mandatory X-Road service
        /// headers for operations.
        /// </summary>
        public string StandardHeaderName { get; set; }

        /// <summary>
        /// Name used for portType element in service description.
        /// </summary>
        public string PortTypeName { get; set; }

        /// <summary>
        /// Name used for binding element in service description.
        /// </summary>
        public string BindingName { get; set; }

        /// <summary>
        /// Name used for port element in service description service element.
        /// </summary>
        public string ServicePortName { get; set; }

        /// <summary>
        /// Name used for service element in service description.
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Name format used for request type naming.
        /// Should contain exactly one string placeholder.
        /// </summary>
        public string RequestTypeNameFormat { get; set; }

        /// <summary>
        /// Name format used for response type naming.
        /// Should contain exactly one string placeholder.
        /// </summary>
        public string ResponseTypeNameFormat { get; set; }

        /// <summary>
        /// Name format used for request message naming.
        /// Should contain exactly one string placeholder.
        /// </summary>
        public string RequestMessageNameFormat { get; set; }

        /// <summary>
        /// Name format used for response message naming.
        /// Should contain exactly one string placeholder.
        /// </summary>
        public string ResponseMessageNameFormat { get; set; }

        /// <summary>
        /// Specifies if service parameter order is strict
        /// (uses sequence) or not (uses all).
        /// </summary>
        public bool StrictOperationSignature { get; set; } = true;

        public bool HasAppliesToValue => appliesTo.HasValue;

        /// <summary>
        /// Specifies protocol version for which this configuration attribute applies to.
        /// When protocol does not have explicit configuration, it uses the one which does not
        /// specify protocol version.
        /// Each protocol can define exactly one configuration attribute. When no matching
        /// configuration is found, exception is thrown.
        /// </summary>
        public XRoadProtocol AppliesTo { get { return appliesTo.GetValueOrDefault(); } set { appliesTo = value; } }

        /// <summary>
        /// Specifies class implementing `IParameterNameProvider` which allows enables to customize parameter names.
        /// </summary>
        public Type ParameterNameProvider { get; set; }
    }
}