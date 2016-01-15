using System;

namespace XRoadLib.Attributes
{
    /// <summary>
    /// Identifies producer which types and service contracts are defined inside attribute target assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class XRoadProducerNameAttribute : Attribute
    {
        /// <summary>
        /// X-Road producer name for contract assembly.
        /// </summary>
        public string Value { get; }

        public XRoadProducerNameAttribute(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));
            Value = value;
        }
    }
}