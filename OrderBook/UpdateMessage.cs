using System;
using System.IO;
using System.Text;

namespace OrderBook
{
    public struct UpdateMessage
    {
        public static UpdateMessage Create(int sequenceNumber, BinaryReader reader)
        {
            var symbol = Encoding.UTF8.GetString(reader.ReadBytes(3));
            var orderId = BitConverter.ToInt32(reader.ReadBytes(8));
            var side = (char) reader.ReadBytes(1)[0] == Constants.Buy ? Side.Buy : Side.Sell;
            reader.ReadBytes(3);
            var size = BitConverter.ToInt32(reader.ReadBytes(8));
            var price = BitConverter.ToInt32(reader.ReadBytes(4));
            reader.ReadBytes(4);
            return new UpdateMessage(sequenceNumber, symbol, orderId, side, size, price);
        }

        internal UpdateMessage(int sequenceNumber, string symbol, int orderId, Side side, int size, int price)
        {
            SequenceNumber = sequenceNumber;
            Symbol = symbol;
            OrderId = orderId;
            Side = side;
            Size = size;
            Price = price;
        }

        public int SequenceNumber { get; }
        public string Symbol { get; }
        public int OrderId { get; }
        public Side Side { get; }
        public int Size { get; }
        public int Price { get; }
    }
}