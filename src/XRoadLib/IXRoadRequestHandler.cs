using System.Threading;
using System.Threading.Tasks;

namespace XRoadLib
{
    public interface IXRoadRequestHandler<in TRequest, TResponse>
        where TRequest : IXRoadRequest<TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}