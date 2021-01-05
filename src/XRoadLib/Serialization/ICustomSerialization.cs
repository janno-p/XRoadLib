using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Serialization
{
    public interface ICustomSerialization
    {
        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        Task OnContentCompleteAsync(XmlWriter writer);
    }
}