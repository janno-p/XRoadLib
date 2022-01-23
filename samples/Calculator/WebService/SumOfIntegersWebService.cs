using Calculator.Contract;

namespace Calculator.WebService;

public class SumOfIntegersWebService : ISumOfIntegers
{
    public int Sum(AddRequest request)
    {
        return request.X + request.Y;
    }
}
