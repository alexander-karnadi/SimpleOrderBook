using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("OrderBook.Tests")]
namespace OrderBook
{
    public class Program
    {
        private const int DefaultLevel = 5; 
        private const string DefaultInputFileName = "input2.stream";
        private const string OutputFileName = "output.log";

        static void Main(string[] args)
        {
            var level = DefaultLevel;
            var inputFileName = DefaultInputFileName;

            try
            {
                if (args.Length > 0)
                {
                    level = int.Parse(args[0]);
                    inputFileName = args[1];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while parsing input. Using default values. Original error: {e}");
            }
            
            var orderBookPrinter = new OrderBookPrinter(level);
            var orderBookView = new OrderBookView(orderBookPrinter);
            
            var watch = System.Diagnostics.Stopwatch.StartNew();

            using (FileStream fs = new FileStream(inputFileName, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(fs))
            using (StreamWriter streamwriter = new StreamWriter(OutputFileName, false, Encoding.UTF8, 65536))
            {
                while (fs.Length != fs.Position)
                {
                    var sequenceNo = BitConverter.ToInt32(reader.ReadBytes(4));
                    reader.ReadBytes(4);
                    var messageType = (char)reader.ReadBytes(1)[0];
                    var output = string.Empty;
                    switch (messageType)
                    {
                        case Constants.Add:
                            var addMessage = AddMessage.Create(sequenceNo, reader);
                            output = orderBookView.Add(addMessage);
                            break;
                        case Constants.Update:
                            var updateMessage = UpdateMessage.Create(sequenceNo, reader);
                            output = orderBookView.Update(updateMessage);
                            break;
                        case Constants.Delete:
                            var deleteMessage = DeleteMessage.Create(sequenceNo, reader);
                            output = orderBookView.Delete(deleteMessage);
                            break;
                        case Constants.Executed:
                            var tradedQuantityMessage = ExecutedMessage.Create(sequenceNo, reader);
                            output = orderBookView.Executed(tradedQuantityMessage);
                            break;
                    }

                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        streamwriter.WriteLine(output);
                    }
                }
            }
            
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Execution finished in {elapsedMs} ms");
        }
    }
}