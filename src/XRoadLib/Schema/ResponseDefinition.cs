using System.Reflection;

namespace XRoadLib.Schema
{
    /// <summary>
    /// Configuration options for customizing response elements.
    /// </summary>
    public class ResponseDefinition : ContentDefinition
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
        /// Describes the appearance of fault elements in service description.
        /// </summary>
        public XRoadFaultPresentation XRoadFaultPresentation { get; set; } = XRoadFaultPresentation.Choice;

        /// <summary>
        /// Name of runtime parameter this response value represents.
        /// </summary>
        public override string RuntimeName => "result";

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
        public string RequestElementName { get; set; } = "request";

        /// <summary>
        /// Response element name in response message.
        /// </summary>
        public string ResponseElementName { get; set; } = "response";

        /// <summary>
        /// Initializes new response definition using default values extracted from
        /// operation definition entity.
        /// </summary>
        public ResponseDefinition(OperationDefinition declaringOperationDefinition)
        {
            var parameterInfo = declaringOperationDefinition.MethodInfo.ReturnParameter;

            DeclaringOperationDefinition = declaringOperationDefinition;
            ParameterInfo = parameterInfo;
            RuntimeType = NormalizeType(parameterInfo?.ParameterType);

            InitializeContentDefinition(parameterInfo);
        }

        /// <summary>
        /// User-friendly display format for response element.
        /// </summary>
        public override string ToString()
        {
            return $"Return value of {ParameterInfo.Member.DeclaringType?.FullName ?? "<null>"}.{ParameterInfo.Member.Name} ({Name})";
        }
    }
}