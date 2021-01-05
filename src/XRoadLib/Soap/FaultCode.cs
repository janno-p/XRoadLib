using System.Diagnostics.CodeAnalysis;

namespace XRoadLib.Soap
{
    public abstract class FaultCode
    {
        protected enum FaultCodeType
        {
            Server,
            Client,
            [SuppressMessage("ReSharper", "UnusedMember.Global")] VersionMismatch,
            [SuppressMessage("ReSharper", "UnusedMember.Global")] MustUnderstand
        }

        public string Value { get; }

        protected FaultCode(FaultCodeType type, string value)
        {
            Value = string.IsNullOrWhiteSpace(value) ? type.ToString() : $"{type}.{value}";
        }
    }
}