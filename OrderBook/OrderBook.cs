using System.Collections.Generic;
using System.Linq;

namespace OrderBook
{
    public interface IOrderBook
    {
        public string Symbol { get; }
        public IEnumerable<(int price, int size)> GetTopBuy(int level);
        public IEnumerable<(int price, int size)> GetTopSell(int level);
    }
    
    public sealed class OrderBook : IOrderBook
    {
        public string Symbol { get; }

        private readonly IDictionary<int, Order> _buyOrders;
        private readonly IDictionary<int, Order> _sellOrders;
        private readonly SortedDictionary<int, int> _buyPriceDepths;
        private readonly SortedDictionary<int, int> _sellPriceDepths;

        public OrderBook(string symbol)
        {
            Symbol = symbol;
            _buyOrders = new Dictionary<int, Order>();
            _sellOrders = new Dictionary<int, Order>();
            _buyPriceDepths = new SortedDictionary<int, int>(new DescendingComparer<int>());
            _sellPriceDepths = new SortedDictionary<int, int>();
        }

        public void Add(AddMessage message)
        {
            var (orders, prices) = GetOrdersAndPriceDepths(message.Side);
            
            var order = new Order(message.OrderId, message.Price, message.Size);
            orders[order.Id] = order;
            
            if (!prices.ContainsKey(message.Price))
            {
                prices[message.Price] = 0;
            }
            
            prices[message.Price] += order.Size;
        }

        public void Update(UpdateMessage message)
        {
            var (orders, prices) = GetOrdersAndPriceDepths(message.Side);
            
            var order = orders[message.OrderId];

            if (message.Price != order.Price)
            {
                prices[order.Price] -= order.Size;
                if (prices[order.Price] == 0)
                {
                    prices.Remove(order.Price);
                }

                if (!prices.ContainsKey(message.Price))
                {
                    prices[message.Price] = 0;
                }
                
                order.Price = message.Price;
            }
            else
            {
                prices[message.Price] -= order.Size;
            }
            
            prices[message.Price] += message.Size;
            
            order.Size = message.Size;
        }

        public void Delete(DeleteMessage message)
        {
            var (orders, prices) = GetOrdersAndPriceDepths(message.Side);

            var order = orders[message.OrderId];
            orders.Remove(order.Id);
            prices[order.Price] -= order.Size;

            if (prices[order.Price] == 0)
            {
                prices.Remove(order.Price);
            }
        }

        public void Executed(ExecutedMessage message)
        {
            var (orders, prices) = GetOrdersAndPriceDepths(message.Side);

            // TODO: for some reason order ID: 95916 for VC5 Buy is added 2x and tried to be executed 2x also
            if (!orders.ContainsKey(message.OrderId))
            {
                return;
            }
            
            var order = orders[message.OrderId];
            order.Size -= message.TradedQuantity;

            if (order.Size == 0)
            {
                orders.Remove(order.Id);
            }
                
            prices[order.Price] -= message.TradedQuantity;
            if (prices[order.Price] == 0)
            {
                prices.Remove(order.Price);
            }
        }

        public IEnumerable<(int price, int size)> GetTopBuy(int level)
        {
            return GetTop(level, _buyPriceDepths);
        }

        public IEnumerable<(int price, int size)> GetTopSell(int level)
        {
            return GetTop(level, _sellPriceDepths);
        }

        private static IEnumerable<(int price, int size)> GetTop(int level, SortedDictionary<int, int> priceDepths)
        {
            var index = 0;
            foreach (var (price, size) in priceDepths)
            {
                if (index < level)
                {
                    if (size > 0)
                    {
                        yield return (price, size);
                        index++;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private (IDictionary<int, Order>, SortedDictionary<int, int>) GetOrdersAndPriceDepths(Side side)
        {
            var orders = side == Side.Buy ? _buyOrders : _sellOrders;
            var prices = side == Side.Buy ? _buyPriceDepths : _sellPriceDepths;

            return (orders, prices);
        }
    }
}