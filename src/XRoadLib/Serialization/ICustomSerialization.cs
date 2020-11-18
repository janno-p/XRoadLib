using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Serialization
{
    public interface ICustomSerialization
    {
        Task OnContentCompleteAsync(XmlWriter writer);
    }
}