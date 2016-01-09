using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class XRoadProducerConfigurationAttribute : Attribute
    {
        private uint? minOperationVersion;
        private uint? maxOperationVersion;

        /// <summary>
        /// Identifies producer which types and service contracts
        /// are defined inside attribute target assembly.
        /// </summary>
        public string ProducerName { get; set; }

        /// <summary>
        /// Minimal version number for operations defined inside
        /// attribute target assembly. Used by versioning subsystem.
        /// </summary>
        public uint MinOperationVersion
        {
            get { return minOperationVersion.GetValueOrDefault(1u); }
            set { minOperationVersion = value; }
        }

        /// <summary>
        /// Maximal version number for operations defined inside
        /// attribute target assembly. Used by versioning subsystem.
        /// </summary>
        public uint MaxOperationVersion
        {
            get { return maxOperationVersion.GetValueOrDefault(1u); }
            set { maxOperationVersion = value; }
        }

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
    }
}