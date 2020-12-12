using System;
using System.IO;
using System.Linq;
using Calculator.Contract;

namespace Calculator.WebService
{
    public class FileCalculationService : IFileCalculation
    {
        public Stream Sum(FileCalculationRequest request)
        {
            if (request.InputFile == null)
                throw new ArgumentNullException(nameof(request.InputFile));

            var resultStream = new MemoryStream();
            var writer = new StreamWriter(resultStream);

            request.InputFile.Position = 0;
            var reader = new StreamReader(request.InputFile);

            var ops = new[] { "+", "-", "*", "/" };

            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                    break;

                var items = line.Split(' ');
                if (items.Length != 3 || !int.TryParse(items[0], out var num1) || !int.TryParse(items[2], out var num2) || !ops.Contains(items[1]))
                {
                    writer.WriteLine("ERR");
                    continue;
                }

                writer.WriteLine(items[1] switch
                {
                    "+" => (num1 + num2).ToString(),
                    "-" => (num1 - num2).ToString(),
                    "*" => (num1 * num2).ToString(),
                    "/" => num2 == 0 ? "ERR" : (num1 / num2).ToString(),
                    _ => "ERR"
                });
            }

            writer.Flush();

            return resultStream;
        }
    }
}