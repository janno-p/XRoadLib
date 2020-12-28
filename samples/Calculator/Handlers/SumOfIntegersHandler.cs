using Calculator.Contract.Operations;
using MediatR;

namespace Calculator.Handlers
{
    public class SumOfIntegersHandler : RequestHandler<SumOfIntegers, int>
    {
        protected override int Handle(SumOfIntegers operation)
        {
            var request = operation.Request;
            return request.X + request.Y;
        }
    }
}