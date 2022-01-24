using System.Diagnostics.CodeAnalysis;
using XRoadLib.Tests.Contract.Wsdl;

namespace XRoadLib.Tests.Contract;

public interface IService
{
    [XRoadService(nameof(Service1))]
    int Service1(Service1Request request);
}

[SuppressMessage("ReSharper", "UnusedType.Global")]
public interface IService2
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [XRoadService(nameof(Service2))]
    int Service2(ContainerType request);
}

public interface IService3
{
    [XRoadService(nameof(Service3))]
    int Service3(TestMergedArrayContent request);
}