using XRoadLib.Serialization;

namespace XRoadLib
{
    public interface IServiceRunner
    {
        object InvokeMetaService(XRoadHttpDataRequest sender, MetaServiceName metaServiceName);

        object GetServiceObject(XRoadHttpDataRequest sender, string name);
    }
}