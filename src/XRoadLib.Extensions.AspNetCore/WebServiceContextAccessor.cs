using System.Diagnostics.CodeAnalysis;

namespace XRoadLib.Extensions.AspNetCore;

public interface IWebServiceContextAccessor
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    WebServiceContext WebServiceContext { get; }
}

public class WebServiceContextAccessor : IWebServiceContextAccessor
{
    public WebServiceContext WebServiceContext { get; set; } = default!;
}