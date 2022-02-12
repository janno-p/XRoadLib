using JetBrains.Annotations;

namespace XRoadLib.Extensions.AspNetCore;

public interface IWebServiceContextAccessor
{
    [UsedImplicitly]
    WebServiceContext WebServiceContext { get; }
}