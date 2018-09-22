namespace XRoadLib.Extensions.AspNetCore
{
    public interface IWebServiceContextAccessor
    {
        WebServiceContext WebServiceContext { get; }
    }
    
    public class WebServiceContextAccessor : IWebServiceContextAccessor
    {
        public WebServiceContext WebServiceContext { get; set; }
    }
}