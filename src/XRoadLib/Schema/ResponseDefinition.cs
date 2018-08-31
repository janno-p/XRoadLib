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
        public XName WrapperElementName { get; set; }

        /// <summary>
        /// Describes the appearance of fault elements in service description.
        /// </summary>
        public XRoadFaultPresentation XRoadFaultPresentation { get; set; } = XRoadFaultPresentation.Choice;

        /// <summary>
        /// Fault element name for response element.
        /// </summary>
        public XName FaultName { get; set; }

        /// <summary>
        /// Should technical fault fields be returned inside response element.
        /// </summary>
        public bool ContainsNonTechnicalFault { get; set; }

        /// <summary>
        /// Request element name in response message.
        /// </summary>
        public XName RequestContentName { get; set; }

        /// <summary>
        /// Element name that is used to wrap operation successful return value.
        /// Used when operation result can be either return value or fault object.
        /// </summary>
        public XName ResultElementName { get; set; }

        /// <summary>
        /// Initializes new response definition using default values extracted from
        /// operation definition entity.
        /// </summary>
        public ResponseDefinition(OperationDefinition declaringOperationDefinition, Func<string, bool> isQualifiedElementDefault)
        {
            var parameterInfo = declaringOperationDefinition.MethodInfo.ReturnParameter;

            DeclaringOperationDefinition = declaringOperationDefinition;
            ParameterInfo = parameterInfo;
            WrapperElementName = XName.Get($"{declaringOperationDefinition.Name.LocalName}Response", declaringOperationDefinition.Name.NamespaceName);

            var targetNamespace = declaringOperationDefinition.Name.NamespaceName;
            var defaultQualifiedElement = isQualifiedElementDefault(targetNamespace);

            Content = ContentDefinition.FromType(
                this,
                parameterInfo,
                parameterInfo?.ParameterType,
                "response",
                targetNamespace,
                defaultQualifiedElement
            );

            var qualifiedNamespace = ContentDefinition.GetQualifiedNamespace("", null, targetNamespace, defaultQualifiedElement);

            FaultName = XName.Get("fault", qualifiedNamespace);
            RequestContentName = XName.Get("request", qualifiedNamespace);
            ResultElementName = XName.Get("result", qualifiedNamespace);
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