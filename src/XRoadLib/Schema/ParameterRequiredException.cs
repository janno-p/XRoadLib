using System.Collections.Generic;
using System.Linq;
using XRoadLib.Soap;

namespace XRoadLib.Schema
{
    public class ParameterRequiredException : ContractViolationException
    {
        public TypeDefinition TypeDefinition { get; }
        public IList<PropertyDefinition> MissingParameters { get; }

        public ParameterRequiredException(string message, TypeDefinition typeDefinition, IList<PropertyDefinition> missingParameters)
            : base(ClientFaultCode.ParameterRequired, message)
        {
            TypeDefinition = typeDefinition;
            MissingParameters = missingParameters;
        }

        public ParameterRequiredException(TypeDefinition typeDefinition, IList<PropertyDefinition> missingParameters)
            : this($"Service input is missing required parameters: {missingParameters.Select(x => x.Content.SerializedName.LocalName)}.", typeDefinition, missingParameters)
        { }

        public ParameterRequiredException(TypeDefinition typeDefinition, PropertyDefinition missingParameter)
            : this($"Service input is missing required parameters: {missingParameter.Content.SerializedName.LocalName}.", typeDefinition, new List<PropertyDefinition> { missingParameter })
        { }
    }
}