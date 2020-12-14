using System;
using Calculator.Contract;
using MediatR;

namespace Calculator.Handlers
{
    public class CalculationRequestHandler : RequestHandler<CalculationRequest, int>
    {
        protected override int Handle(CalculationRequest request)
        {
            var operation = GetOperation(request.Operation);
            return operation(request.X, request.Y);
        }

        private static Func<int, int, int> GetOperation(Operation operation) => operation switch
        {
            Operation.Add => (x, y) => x + y,
            Operation.Subtract => (x, y) => x - y,
            Operation.Multiply => (x, y) => x * y,
            Operation.Divide => (x, y) => x / y,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
    }
}