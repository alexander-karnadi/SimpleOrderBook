using System.Collections.Generic;

namespace OrderBook
{
    public sealed class OrderBookView
    {
        private readonly IOrderBookPrinter _printer;
        private readonly IDictionary<string, OrderBook> _orderBooks;

        public OrderBookView(IOrderBookPrinter printer)
        {
            _printer = printer;
            _orderBooks = new Dictionary<string, OrderBook>();
        }

        public string Add(AddMessage addMessage)
        {
            if (!_orderBooks.ContainsKey(addMessage.Symbol))
            {
                _orderBooks[addMessage.Symbol] = new OrderBook(addMessage.Symbol);
            }
            
            _orderBooks[addMessage.Symbol].Add(addMessage);

            return _printer.Print(addMessage.SequenceNumber, _orderBooks[addMessage.Symbol]);
        }

        public string Update(UpdateMessage updateMessage)
        {
            _orderBooks[updateMessage.Symbol].Update(updateMessage);
            return _printer.Print(updateMessage.SequenceNumber, _orderBooks[updateMessage.Symbol]);
        }
        
        public string Delete(DeleteMessage deletedMessage)
        {
            _orderBooks[deletedMessage.Symbol].Delete(deletedMessage);
            return _printer.Print(deletedMessage.SequenceNumber, _orderBooks[deletedMessage.Symbol]);
        }
        
        public string Executed(ExecutedMessage executedMessage)
        {
            _orderBooks[executedMessage.Symbol].Executed(executedMessage);
            return _printer.Print(executedMessage.SequenceNumber, _orderBooks[executedMessage.Symbol]);
        }
    }
}