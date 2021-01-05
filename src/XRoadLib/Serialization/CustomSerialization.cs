using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Serialization
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class CustomSerialization : ICustomSerialization
    {
        public virtual Task OnContentCompleteAsync(XmlWriter writer) =>
            Task.CompletedTask;
    }
}