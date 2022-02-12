﻿using System.Reflection;

namespace XRoadLib.Schema;

/// <summary>
/// Specification for individual X-Road message request part.
/// </summary>
public class RequestDefinition : ParticleDefinition
{
    /// <summary>
    /// Operation which uses this request part in its input.
    /// </summary>
    [UsedImplicitly]
    public OperationDefinition DeclaringOperationDefinition { get; }

    /// <summary>
    /// Runtime parameter info of request object.
    /// </summary>
    public ParameterInfo ParameterInfo { get; }

    /// <summary>
    /// Wrapper element name for incoming requests.
    /// </summary>
    [UsedImplicitly]
    public XName WrapperElementName { get; set; }

    /// <summary>
    /// Initializes new request definition object.
    /// </summary>
    public RequestDefinition(OperationDefinition declaringOperationDefinition, Func<string, bool> isQualifiedElementDefault)
    {
        var methodParameters = declaringOperationDefinition.MethodInfo.GetParameters();
        if (methodParameters.Length > 1)
            throw new SchemaDefinitionException($"Invalid X-Road operation contract `{declaringOperationDefinition.Name.LocalName}`: expected 0-1 input parameters, but {methodParameters.Length} was given.");

        DeclaringOperationDefinition = declaringOperationDefinition;
        ParameterInfo = methodParameters.SingleOrDefault();
        WrapperElementName = declaringOperationDefinition.Name;

        var targetNamespace = declaringOperationDefinition.Name.NamespaceName;
        var defaultQualifiedElement = isQualifiedElementDefault(targetNamespace);

        Content = ContentDefinition.FromType(
            this,
            ParameterInfo,
            ParameterInfo?.ParameterType,
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
        return $"Input value of {ParameterInfo.Member.DeclaringType?.FullName ?? "<null>"}.{ParameterInfo.Member.Name}";
    }
}