using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    /// <summary>
    /// Specification for individual X-Road message request part.
    /// </summary>
    public class RequestDefinition : ParticleDefinition
    {
        /// <summary>
        /// Operation which uses this request part in its input.
        /// </summary>
        public OperationDefinition DeclaringOperationDefinition { get; }

        /// <summary>
        /// Runtime parameter info of request object.
        /// </summary>
        public Type RequestType { get; }

        /// <summary>
        /// Wrapper element name for incoming requests.
        /// </summary>
        public XName WrapperElementName { get; set; }

        /// <summary>
        /// Initializes new request definition object.
        /// </summary>
        public RequestDefinition(OperationDefinition declaringOperationDefinition, Func<string, bool> isQualifiedElementDefault)
        {
            DeclaringOperationDefinition = declaringOperationDefinition;
            RequestType = DeclaringOperationDefinition.RequestType;
            WrapperElementName = declaringOperationDefinition.Name;

            var targetNamespace = declaringOperationDefinition.Name.NamespaceName;
            var defaultQualifiedElement = isQualifiedElementDefault(targetNamespace);

            Content = ContentDefinition.FromType(
                this,
                RequestType,
                RequestType,
                "request",
                targetNamespace,
                defaultQualifiedElement
            );
        }

        /// <summary>
        /// Detailed string presentation of the request object.
        /// </summary>
        public override string ToString()
        {
            return $"Input value of operation {DeclaringOperationDefinition.Name} ({RequestType.FullName})";
        }
    }
}