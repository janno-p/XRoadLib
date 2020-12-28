using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Calculator.Contract.Operations;
using MediatR;

namespace Calculator.Handlers
{
    public class FileCalculationHandler : IRequestHandler<FileCalculation, Stream>
    {
        public async Task<Stream> Handle(FileCalculation operation, CancellationToken cancellationToken)
        {
            var request = operation.Request;

            if (request.InputFile == null)
                throw new ArgumentNullException(nameof(request.InputFile));

            var resultStream = new MemoryStream();
            var writer = new StreamWriter(resultStream);

            request.InputFile.Position = 0;
            var reader = new StreamReader(request.InputFile);

            var ops = new[] { "+", "-", "*", "/" };

            while (true)
            {
                var line = await reader.ReadLineAsync();
                if (line == null)
                    break;

                var items = line.Split(' ');
                if (items.Length != 3 || !int.TryParse(items[0], out var num1) || !int.TryParse(items[2], out var num2) || !ops.Contains(items[1]))
                {
                    await writer.WriteLineAsync("ERR");
                    continue;
                }

                await writer.WriteLineAsync(items[1] switch
                {
                    "+" => (num1 + num2).ToString(),
                    "-" => (num1 - num2).ToString(),
                    "*" => (num1 * num2).ToString(),
                    "/" => num2 == 0 ? "ERR" : (num1 / num2).ToString(),
                    _ => "ERR"
                });
            }

            await writer.FlushAsync();

            return resultStream;
        }
    }
}