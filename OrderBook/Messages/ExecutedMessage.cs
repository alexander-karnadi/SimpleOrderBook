using System;
using System.IO;
using System.Text;

namespace OrderBook
{
    public struct ExecutedMessage
    {
        public static ExecutedMessage Create(int sequenceNumber, BinaryReader reader)
        {
            var symbol = Encoding.UTF8.GetString(reader.ReadBytes(3));
            var orderId = BitConverter.ToInt32(reader.ReadBytes(8));
            var side = (char) reader.ReadBytes(1)[0] == Constants.Buy ? Side.Buy : Side.Sell;
            reader.ReadBytes(3);
            var tradedQuantity = BitConverter.ToInt32(reader.ReadBytes(8));
            return new ExecutedMessage(sequenceNumber, symbol, orderId, side, tradedQuantity);
        }

        internal ExecutedMessage(int sequenceNumber, string symbol, int orderId, Side side, int tradedQuantity)
        {
            SequenceNumber = sequenceNumber;
            Symbol = symbol;
            OrderId = orderId;
            Side = side;
            TradedQuantity = tradedQuantity;
        }

        public int SequenceNumber { get; }
        public string Symbol { get; }
        public int OrderId { get; }
        public Side Side { get; }
        public int TradedQuantity { get; }
    }
}