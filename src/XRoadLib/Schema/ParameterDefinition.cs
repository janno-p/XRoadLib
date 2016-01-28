using System;
using System.Reflection;

namespace XRoadLib.Schema
{
    public class ParameterDefinition : ContentDefinition<ParameterInfo>
    {
        public OperationDefinition Owner { get; }

        public ParameterDefinition(OperationDefinition owner)
        {
            Owner = owner;
        }

        public override string ContainerName => $"{RuntimeInfo.Member.DeclaringType?.FullName}.{RuntimeInfo.Member.Name}";
        public override string RuntimeName => RuntimeInfo.Name;
        public override Type RuntimeType => RuntimeInfo.ParameterType;
    }
}