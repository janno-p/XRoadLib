using System;
using System.Linq;
using System.Reflection;

namespace XRoadLib.Schema
{
    /// <summary>
    /// Specification for individual X-Road message request part.
    /// </summary>
    public class RequestDefinition : ContentDefinition
    {
        /// <summary>
        /// Operation which uses this request part in its input.
        /// </summary>
        public OperationDefinition DeclaringOperationDefinition { get; }

        /// <summary>
        /// Runtime parameter info of request object.
        /// </summary>
        public ParameterInfo ParameterInfo { get; }

        /// <summary>
        /// Runtime name to identify this definition object.
        /// </summary>
        public override string RuntimeName => "request";

        /// <summary>
        /// Serialized element name of this request object.
        /// </summary>
        public string RequestElementName { get; set; } = "request";

        /// <summary>
        /// Initializes new request definition object.
        /// </summary>
        public RequestDefinition(OperationDefinition declaringOperationDefinition)
        {
            var methodParameters = declaringOperationDefinition.MethodInfo.GetParameters();
            if (methodParameters.Length > 1)
                throw new Exception($"Invalid X-Road operation contract `{declaringOperationDefinition.Name.LocalName}`: expected 0-1 input parameters, but {methodParameters.Length} was given.");

            DeclaringOperationDefinition = declaringOperationDefinition;
            ParameterInfo = methodParameters.SingleOrDefault();

            if (ParameterInfo == null)
                return;

            RuntimeType = NormalizeType(ParameterInfo.ParameterType);

            InitializeContentDefinition(ParameterInfo);
        }

        /// <summary>
        /// Detailed string presentation of the request object.
        /// </summary>
        public override string ToString()
        {
            return $"Input value of {ParameterInfo.Member.DeclaringType?.FullName ?? "<null>"}.{ParameterInfo.Member.Name} ({Name})";
        }
    }
}