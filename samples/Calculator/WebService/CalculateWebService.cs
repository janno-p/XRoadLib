using System;
using System.Threading.Tasks;
using Calculator.Contract;

namespace Calculator.WebService
{
    public class CalculateWebService : ICalculate
    {
        public int Calculate(CalculationRequest request)
        {
            return GetOperation(request.Operation)(request.X, request.Y);
        }

        public Task<int> CalculateAsync(CalculationRequest request)
        {
            var result = GetOperation(request.Operation)(request.X, request.Y);
            return Task.FromResult(result);
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