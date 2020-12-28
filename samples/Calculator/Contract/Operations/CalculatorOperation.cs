using MediatR;
using XRoadLib;
using XRoadLib.Attributes;
using XRoadLib.Headers;

namespace Calculator.Contract.Operations
{
    [XRoadSerializable]
    public abstract class CalculatorOperation<TRequest, TResponse> : XRoadOperation<TRequest, TResponse, XRoadHeader>, IRequest<TResponse>
    { }
}