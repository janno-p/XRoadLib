using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Serialization
{
    public class CustomSerialization : ICustomSerialization
    {
        public virtual Task OnContentCompleteAsync(XmlWriter writer) =>
            Task.CompletedTask;
    }
}