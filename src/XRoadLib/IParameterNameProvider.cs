using System.Reflection;

namespace XRoadLib
{
    public interface IParameterNameProvider
    {
        string GetParameterName(ParameterInfo parameterContract, ParameterInfo parameterImpl);
    }
}