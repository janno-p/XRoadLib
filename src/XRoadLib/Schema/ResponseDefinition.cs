using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using XRoadLib.Extensions;

namespace XRoadLib.Schema
{
    /// <summary>
    /// Configuration options for customizing response elements.
    /// </summary>
    public class ResponseDefinition : ParticleDefinition
    {
        private static readonly Type TaskType = typeof(Task);
        private static readonly Type GenericTaskType = typeof(Task<>);

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
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public XName WrapperElementName { get; set; }

        /// <summary>
        /// Describes the appearance of fault elements in service description.
        /// </summary>
        public XRoadFaultPresentation XRoadFaultPresentation { get; set; } = XRoadFaultPresentation.Choice;

        /// <summary>
        /// Fault element name for response element.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public XName FaultName { get; set; }

        /// <summary>
        /// Should technical fault fields be returned inside response element.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public bool ContainsNonTechnicalFault { get; set; }

        /// <summary>
        /// Request element name in response message.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public XName RequestContentName { get; set; }

        /// <summary>
        /// Element name that is used to wrap operation successful return value.
        /// Used when operation result can be either return value or fault object.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public XName ResultElementName { get; set; }

        /// <summary>
        /// Indicates that method uses async calls.
        /// </summary>
        public bool IsAsync { get; }

        /// <summary>
        /// Used in serializer to convert from specific type task to Task&lt;object&gt;
        /// </summary>
        public ConvertTaskMethod ConvertTaskMethod { get; set; }

        /// <summary>
        /// Initializes new response definition using default values extracted from
        /// operation definition entity.
        /// </summary>
        public ResponseDefinition(OperationDefinition declaringOperationDefinition, Func<string, bool> isQualifiedElementDefault)
        {
            var parameterInfo = declaringOperationDefinition.MethodInfo.ReturnParameter;

            var parameterType = parameterInfo?.ParameterType;
            if (parameterType is not null)
            {
                if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == GenericTaskType)
                {
                    IsAsync = true;
                    parameterType = parameterType.GetGenericArguments().Single();
                    ConvertTaskMethod = parameterInfo.CreateConvertTaskMethod();
                }
                else if (parameterType == TaskType)
                {
                    IsAsync = true;
                    parameterType = typeof(void);
                    ConvertTaskMethod = parameterInfo.CreateConvertTaskMethod();
                }
            }

            DeclaringOperationDefinition = declaringOperationDefinition;
            ParameterInfo = parameterInfo;
            WrapperElementName = XName.Get($"{declaringOperationDefinition.Name.LocalName}Response", declaringOperationDefinition.Name.NamespaceName);

            var targetNamespace = declaringOperationDefinition.Name.NamespaceName;
            var defaultQualifiedElement = isQualifiedElementDefault(targetNamespace);

            Content = ContentDefinition.FromType(
                this,
                parameterInfo,
                parameterType,
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