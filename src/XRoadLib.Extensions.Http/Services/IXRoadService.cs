using System.Threading.Tasks;
using XRoadLib.Headers;

namespace XRoadLib.Extensions.Http.Services
{
    public interface IXRoadService
    {
        /// <summary>
        /// Executes X-Road operation on endpoint specified by WebRequest parameter.
        /// </summary>
        /// <param name="body">Soap body part of outgoing serialized X-Road message.</param>
        /// <param name="header">Soap header part of outgoing serialized X-Road message.</param>
        /// <param name="options">Additional options to configure service call execution.</param>
        /// <returns>Deserialized value of X-Road response message Soap body.</returns>
        Task<XRoadResponse> ExecuteAsync(object body, ISoapHeader header, ServiceExecutionOptions options = null);
    }
}