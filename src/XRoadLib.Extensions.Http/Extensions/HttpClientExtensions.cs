using System.Net.Http;
using System.Threading.Tasks;
using XRoadLib.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Extensions.Http.Extensions
{
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Serializes X-Road message into specified web request.
        /// </summary>
        public static async Task<HttpResponseMessage> SendXRoadMessageAsync(this HttpClient httpClient, XRoadMessage message, IMessageFormatter messageFormatter)
        {
            using var httpContent = message.GetHttpContent(messageFormatter);

            httpContent.Headers.Add("SOAPAction", string.Empty);

            return await httpClient.PostAsync("", httpContent).ConfigureAwait(false);
        }
    }
}