using System.Runtime.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Schema;

[Serializable]
public class ParameterRequiredException : ContractViolationException
{
    [UsedImplicitly]
    public TypeDefinition TypeDefinition { get; }

    [UsedImplicitly]
    public IList<PropertyDefinition> MissingParameters { get; }

    [UsedImplicitly]
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

    protected ParameterRequiredException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}