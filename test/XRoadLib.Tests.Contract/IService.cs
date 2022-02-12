using JetBrains.Annotations;
using XRoadLib.Tests.Contract.Wsdl;

namespace XRoadLib.Tests.Contract;

public interface IService
{
    [XRoadService(nameof(Service1))]
    int Service1(Service1Request request);
}

[UsedImplicitly]
public interface IService2
{
    [UsedImplicitly]
    [XRoadService(nameof(Service2))]
    int Service2(ContainerType request);
}

public interface IService3
{
    [XRoadService(nameof(Service3))]
    int Service3(TestMergedArrayContent request);
}