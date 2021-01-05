using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using XRoadLib.Soap;

namespace XRoadLib.Schema
{
    public class ParameterRequiredException : ContractViolationException
    {
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public TypeDefinition TypeDefinition { get; }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public IList<PropertyDefinition> MissingParameters { get; }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public ParameterRequiredException(string message, TypeDefinition typeDefinition, IList<PropertyDefinition> missingParameters)
            : base(ClientFaultCode.ParameterRequired, message)
        {
            TypeDefinition = typeDefinition;
            MissingParameters = missingParameters;
        }

        public ParameterRequiredException(TypeDefinition typeDefinition, IList<PropertyDefinition> missingParameters)
            : this($"Service input is missing required parameters: {string.Join(", ", missingParameters.Select(x => $"`{x.Content.SerializedName.LocalName}`"))}.", typeDefinition, missingParameters)
        { }

        public ParameterRequiredException(TypeDefinition typeDefinition, PropertyDefinition missingParameter)
            : this($"Service input is missing required parameters: `{missingParameter.Content.SerializedName.LocalName}`.", typeDefinition, new List<PropertyDefinition> { missingParameter })
        { }
    }
}