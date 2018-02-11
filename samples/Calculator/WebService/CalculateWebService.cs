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

        private Func<int, int, int> GetOperation(Operation operation)
        {
            switch (operation)
            {
                case Operation.Add:
                    return (x, y) => x + y;

                case Operation.Subtract:
                    return (x, y) => x - y;

                case Operation.Multiply:
                    return (x, y) => x * y;

                case Operation.Divide:
                    return (x, y) => x / y;

                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
            }
        }
    }
}