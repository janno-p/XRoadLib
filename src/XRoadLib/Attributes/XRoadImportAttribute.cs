using System;

namespace XRoadLib.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class XRoadImportAttribute : Attribute
    {
        public string RequestPart { get; }
        public string ResponsePart { get; }
        public Type SchemaImportProvider { get; }

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