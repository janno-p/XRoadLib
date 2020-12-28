using System.Threading.Tasks;
using XRoadLib.Headers;

namespace XRoadLib.Extensions.Http.Services
{
    public interface IXRoadService
    {
        /// <summary>
        /// Executes X-Road operation on endpoint specified by WebRequest parameter.
        /// </summary>
        /// <param name="operation">Soap envelope contents of outgoing serialized X-Road message.</param>
        /// <param name="options">Additional options to configure service call execution.</param>
        /// <returns>Deserialized value of X-Road response message Soap body.</returns>
        Task<XRoadResponse<TResult>> RunOperationAsync<TRequest, TResult, THeader>(XRoadOperation<TRequest, TResult, THeader> operation, ServiceExecutionOptions options = null)
            where THeader : ISoapHeader;
    }
}