using System;
using System.Reflection;
using XRoadLib.Serialization;

namespace XRoadLib
{
    public interface IServiceRunner
    {
        object InvokeMetaService(XRoadHttpDataRequest sender, MetaServiceName metaServiceName);

        Tuple<object, MethodInfo> GetDataService(XRoadHttpDataRequest sender, string name);
    }
}