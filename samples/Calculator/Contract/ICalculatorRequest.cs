using MediatR;
using XRoadLib;

namespace Calculator.Contract
{
    public interface ICalculatorRequest<T> : IXRoadRequest<T>, IRequest<T> { }
}