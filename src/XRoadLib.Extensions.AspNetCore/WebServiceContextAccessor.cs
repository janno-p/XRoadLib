namespace XRoadLib.Extensions.AspNetCore;

public class WebServiceContextAccessor : IWebServiceContextAccessor
{
    public WebServiceContext WebServiceContext { get; set; } = default!;
}