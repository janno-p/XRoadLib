using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Calculator.Contract.Operations;
using MediatR;

namespace Calculator.Handlers
{
    public class FileTransferRequestHandler : IRequestHandler<FileTransfer, Stream>
    {
        public async Task<Stream> Handle(FileTransfer operation, CancellationToken cancellationToken)
        {
            var stream = new MemoryStream();

            var request = operation.Request;

            request.Input.Position = 0;
            await request.Input.CopyToAsync(stream, cancellationToken);

            return stream;
        }
    }
}