using System;
using System.Reflection;
using XRoadLib.Serialization;

namespace XRoadLib
{
    public interface IServiceRunner
    {
        object InvokeMetaService(MetaServiceName metaServiceName);

        Tuple<object, MethodInfo> GetDataService(string name);
    }
}