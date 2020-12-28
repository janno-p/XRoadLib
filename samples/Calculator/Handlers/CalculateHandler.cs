using System;
using Calculator.Contract.Operations;
using Calculator.Contract.Types;
using MediatR;

namespace Calculator.Handlers
{
    public class CalculateHandler : RequestHandler<Calculate, int>
    {
        protected override int Handle(Calculate operation)
        {
            var request = operation.Request;
            var @operator = GetOperation(request.Operator);
            return @operator(request.X, request.Y);
        }

        private static Func<int, int, int> GetOperation(Operator @operator) => @operator switch
        {
            Operator.Add => (x, y) => x + y,
            Operator.Subtract => (x, y) => x - y,
            Operator.Multiply => (x, y) => x * y,
            Operator.Divide => (x, y) => x / y,
            _ => throw new ArgumentOutOfRangeException(nameof(@operator), @operator, null)
        };
    }
}