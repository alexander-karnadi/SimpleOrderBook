using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrderBook
{
    public interface IOrderBookPrinter
    {
        string Print(int seqNumber, IOrderBook orderBook);
    }
    
    public sealed class OrderBookPrinter : IOrderBookPrinter
    {
        private readonly IDictionary<string, HashSet<int>> _lastTopBuy = new Dictionary<string, HashSet<int>>();
        private readonly IDictionary<string, HashSet<int>> _lastTopSell = new Dictionary<string, HashSet<int>>();
        private readonly int _level;

        public OrderBookPrinter(int level)
        {
            _level = level;
        }
        
        public string Print(int seqNumber, IOrderBook orderBook)
        {
            var latestTopBuy = orderBook.GetTopBuy(_level).ToList();
            var latestTopSell = orderBook.GetTopSell(_level).ToList();
            var latestTopBuyHash = latestTopBuy.Select(_ => _.GetHashCode()).ToHashSet();
            var latestTopSellHash = latestTopSell.Select(_ => _.GetHashCode()).ToHashSet();

            if (_lastTopBuy.ContainsKey(orderBook.Symbol)
                && _lastTopSell.ContainsKey(orderBook.Symbol)
                && latestTopBuyHash.SetEquals(_lastTopBuy[orderBook.Symbol])
                && latestTopSellHash.SetEquals(_lastTopSell[orderBook.Symbol])
            )
            {
                return string.Empty;
            }

            _lastTopBuy[orderBook.Symbol] = latestTopBuyHash;
            _lastTopSell[orderBook.Symbol] = latestTopSellHash;
            
            var stringBuilder = new StringBuilder();
            stringBuilder
                .Append(seqNumber)
                .Append(", ")
                .Append(orderBook.Symbol)
                .Append(", [")
                .Append(string.Join(", ", latestTopBuy))
                .Append("], [")
                .Append(string.Join(", ", latestTopSell))
                .Append("]");

            return stringBuilder.ToString();
        }
    }
}