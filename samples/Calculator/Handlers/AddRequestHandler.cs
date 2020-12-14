using Calculator.Contract;
using MediatR;

namespace Calculator.Handlers
{
    public class AddRequestHandler : RequestHandler<AddRequest, int>
    {
        protected override int Handle(AddRequest request)
        {
            return request.X + request.Y;
        }
    }
}