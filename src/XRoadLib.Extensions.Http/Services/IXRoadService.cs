using System.Threading.Tasks;
using XRoadLib.Headers;

namespace XRoadLib.Extensions.Http.Services
{
    public interface IXRoadService
    {
        /// <summary>
        /// Executes X-Road operation on endpoint specified by WebRequest parameter.
        /// </summary>
        /// <param name="request">Soap body part of outgoing serialized X-Road message.</param>
        /// <param name="header">Soap header part of outgoing serialized X-Road message.</param>
        /// <param name="options">Additional options to configure service call execution.</param>
        /// <returns>Deserialized value of X-Road response message Soap body.</returns>
        Task<XRoadResponse<TResult>> ExecuteAsync<TResult>(IXRoadRequest<TResult> request, ISoapHeader header, ServiceExecutionOptions options = null);
    }
}