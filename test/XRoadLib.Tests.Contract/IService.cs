using XRoadLib.Attributes;
using XRoadLib.Tests.Contract.Wsdl;

namespace XRoadLib.Tests.Contract
{
    public interface IService
    {
        [XRoadService("Service1")]
        int Service1(Service1Request request);
    }

    public interface IService2
    {
        [XRoadService("Service2")]
        int Service2(ContainerType request);
    }

    public interface IService3
    {
        [XRoadService("Service3")]
        int Service3(TestMergedArrayContent request);
    }
}