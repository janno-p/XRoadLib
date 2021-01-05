using System;
using Calculator.Contract;

namespace Calculator.WebService
{
    public class CalculateWebService : ICalculate
    {
        public int Calculate(CalculationRequest request)
        {
            return GetOperation(request.Operation)(request.X, request.Y);
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