using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class XRoadImportAttribute : Attribute, IXRoadProtocolAppliable
    {
        private XRoadProtocol? appliesTo;

        public string RequestPart { get; }
        public string ResponsePart { get; }
        public Type SchemaImportProvider { get; }

        public bool HasAppliesToValue => appliesTo.HasValue;

        public XRoadProtocol AppliesTo { get { return appliesTo.GetValueOrDefault(); } set { appliesTo = value; } }

        public XRoadImportAttribute(string requestPart, string responsePart, Type schemaImportProvider)
        {
            RequestPart = requestPart;
            ResponsePart = responsePart;

            if (schemaImportProvider == null)
                throw new ArgumentNullException(nameof(schemaImportProvider));
            SchemaImportProvider = schemaImportProvider;
        }
    }
}