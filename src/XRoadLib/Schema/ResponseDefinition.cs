using System;
using System.Reflection;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    /// <summary>
    /// Configuration options for customizing response elements.
    /// </summary>
    public class ResponseDefinition : ParticleDefinition
    {
        /// <summary>
        /// Operation definition to which this response element definition belongs to.
        /// </summary>
        public OperationDefinition DeclaringOperationDefinition { get; }

        /// <summary>
        /// Runtime return parameter of the method which implements the operation.
        /// </summary>
        public ParameterInfo ParameterInfo { get; }

        /// <summary>
        /// Wrapper element name for outgoing responses.
        /// </summary>
        public string WrapperElementName { get; set; }

        /// <summary>
        /// Describes the appearance of fault elements in service description.
        /// </summary>
        public XRoadFaultPresentation XRoadFaultPresentation { get; set; } = XRoadFaultPresentation.Choice;

        /// <summary>
        /// Fault element name for response element.
        /// </summary>
        public string FaultName { get; set; } = "fault";

        /// <summary>
        /// Should technical fault fields be returned inside response element.
        /// </summary>
        public bool ContainsNonTechnicalFault { get; set; } = false;

        /// <summary>
        /// Request element name in response message.
        /// </summary>
        public XName RequestElementName { get; set; }

        /// <summary>
        /// Response element name in response message.
        /// </summary>
        public XName ResponseElementName { get; set; }

        /// <summary>
        /// Initializes new response definition using default values extracted from
        /// operation definition entity.
        /// </summary>
        public ResponseDefinition(OperationDefinition declaringOperationDefinition, Func<string, bool> isQualifiedElementDefault)
        {
            var parameterInfo = declaringOperationDefinition.MethodInfo.ReturnParameter;

            DeclaringOperationDefinition = declaringOperationDefinition;
            ParameterInfo = parameterInfo;

            var targetNamespace = declaringOperationDefinition.Name.NamespaceName;
            var defaultQualifiedElement = isQualifiedElementDefault(targetNamespace);

            Content = ContentDefinition.FromType(
                this,
                parameterInfo,
                parameterInfo?.ParameterType,
                "result",
                targetNamespace,
                defaultQualifiedElement
            );

            var qualifiedNamespace = ContentDefinition.GetQualifiedNamespace("", null, targetNamespace, defaultQualifiedElement);

            RequestElementName = XName.Get("request", qualifiedNamespace);
            ResponseElementName = XName.Get("response", qualifiedNamespace);
        }

        /// <summary>
        /// User-friendly display format for response element.
        /// </summary>
        public override string ToString()
        {
            return $"Return value of {ParameterInfo.Member.DeclaringType?.FullName ?? "<null>"}.{ParameterInfo.Member.Name}";
        }
    }
}