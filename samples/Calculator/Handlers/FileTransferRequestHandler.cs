using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Calculator.Contract;
using MediatR;

namespace Calculator.Handlers
{
    public class FileTransferRequestHandler : IRequestHandler<FileTransferRequest, Stream>
    {
        public async Task<Stream> Handle(FileTransferRequest request, CancellationToken cancellationToken)
        {
            var stream = new MemoryStream();

            request.Input.Position = 0;
            await request.Input.CopyToAsync(stream, cancellationToken);

            return stream;
        }
    }
}