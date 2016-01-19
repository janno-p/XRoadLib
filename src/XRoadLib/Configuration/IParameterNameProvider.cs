using System.Reflection;

namespace XRoadLib.Configuration
{
    public interface IParameterNameProvider
    {
        string GetParameterName(ParameterInfo parameterContract, string operationName);
    }
}