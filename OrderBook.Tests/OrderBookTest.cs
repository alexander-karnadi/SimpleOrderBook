using System;
using System.Linq;
using Xunit;
using Xunit.Sdk;

namespace OrderBook.Tests
{
    public class OrderBookTest
    {
        private const string TestSymbol = "Tst";
        private const int Level = 5;
        
        [Fact]
        public void AddTest()
        {
            var sut = BuildOrderBook();

            var topBuys = sut.GetTopBuy(Level).ToList();
            var topSells = sut.GetTopSell(Level).ToList();
            
            Assert.Equal(5, topBuys.Count);
            Assert.Equal((109, 3), topBuys[0]);
            Assert.Equal((105, 222), topBuys[1]);
            Assert.Equal((104, 44), topBuys[2]);
            Assert.Equal((103, 234), topBuys[3]);
            Assert.Equal((102, 100), topBuys[4]);
            
            Assert.Equal(2, topSells.Count);
            Assert.Equal((110, 200), topSells[0]);
            Assert.Equal((111, 10), topSells[1]);
        }
        
        [Fact]
        public void UpdateTest()
        {
            var sut = BuildOrderBook();
            
            sut.Update(new UpdateMessage(1012, TestSymbol, 5, Side.Buy, 240, 105));
            sut.Update(new UpdateMessage(1013, TestSymbol, 7, Side.Buy, 300, 105));
            sut.Update(new UpdateMessage(1014, TestSymbol, 1, Side.Buy, 100, 109));
            sut.Update(new UpdateMessage(1015, TestSymbol, 1, Side.Buy, 50, 109));
            sut.Update(new UpdateMessage(1016, TestSymbol, 10, Side.Buy, 100, 103));
            sut.Update(new UpdateMessage(1017, TestSymbol, 6, Side.Sell, 123, 150));
            sut.Update(new UpdateMessage(1018, TestSymbol, 7, Side.Buy, 100, 103));
            sut.Update(new UpdateMessage(1019, TestSymbol, 3, Side.Buy, 100, 109));

            var topBuys = sut.GetTopBuy(Level).ToList();
            var topSells = sut.GetTopSell(Level).ToList();
            
            Assert.Equal(4, topBuys.Count);
            Assert.Equal((109, 153), topBuys[0]);
            Assert.Equal((105, 240), topBuys[1]);
            Assert.Equal((104, 44), topBuys[2]);
            Assert.Equal((103, 434), topBuys[3]);
            
            Assert.Equal(2, topSells.Count);
            Assert.Equal((110, 200), topSells[0]);
            Assert.Equal((150, 123), topSells[1]);
        }
        
        [Fact]
        public void DeletedTest()
        {
            var sut = BuildOrderBook();

            sut.Delete(new DeleteMessage(1001, TestSymbol, 7, Side.Buy));
            sut.Delete(new DeleteMessage(1001, TestSymbol, 10, Side.Buy));
            sut.Delete(new DeleteMessage(1001, TestSymbol, 6, Side.Sell));

            var topBuys = sut.GetTopBuy(Level).ToList();
            var topSells = sut.GetTopSell(Level).ToList();
            
            Assert.Equal(5, topBuys.Count);
            Assert.Equal((109, 3), topBuys[0]);
            Assert.Equal((105, 222), topBuys[1]);
            Assert.Equal((104, 44), topBuys[2]);
            Assert.Equal((103, 234), topBuys[3]);
            Assert.Equal((100, 200), topBuys[4]);
            
            Assert.Single(topSells);
            Assert.Equal((110, 200), topSells[0]);
        }
        
        [Fact]
        public void ExecutedTest()
        {
            var sut = BuildOrderBook();

            sut.Executed(new ExecutedMessage(1001, TestSymbol, 1, Side.Buy, 99));
            sut.Executed(new ExecutedMessage(1001, TestSymbol, 6, Side.Sell, 10));
            sut.Executed(new ExecutedMessage(1001, TestSymbol, 11, Side.Buy, 2));
            sut.Executed(new ExecutedMessage(1001, TestSymbol, 7, Side.Buy, 100));
            sut.Executed(new ExecutedMessage(1001, TestSymbol, 10, Side.Buy, 100));

            var topBuys = sut.GetTopBuy(Level).ToList();
            var topSells = sut.GetTopSell(Level).ToList();
            
            Assert.Equal(5, topBuys.Count);
            Assert.Equal((109, 1), topBuys[0]);
            Assert.Equal((105, 222), topBuys[1]);
            Assert.Equal((104, 44), topBuys[2]);
            Assert.Equal((103, 234), topBuys[3]);
            Assert.Equal((100, 101), topBuys[4]);
            
            Assert.Single(topSells);
            Assert.Equal((110, 200), topSells[0]);
        }
        
        [Fact]
        public void MixedOperationsTest()
        {
            var sut = BuildOrderBook();

            sut.Executed(new ExecutedMessage(1001, TestSymbol, 1, Side.Buy, 99));
            sut.Update(new UpdateMessage(1001, TestSymbol, 6, Side.Sell, 10, 220));
            sut.Add(new AddMessage(1001, TestSymbol, 12, Side.Sell, 55, 112));
            sut.Executed(new ExecutedMessage(1001, TestSymbol, 9, Side.Buy, 4));
            sut.Delete(new DeleteMessage(1001, TestSymbol, 6, Side.Sell));

            var topBuys = sut.GetTopBuy(10).ToList();
            var topSells = sut.GetTopSell(10).ToList();
            
            Assert.Equal(7, topBuys.Count);
            Assert.Equal((109, 3), topBuys[0]);
            Assert.Equal((105, 222), topBuys[1]);
            Assert.Equal((104, 40), topBuys[2]);
            Assert.Equal((103, 234), topBuys[3]);
            Assert.Equal((102, 100), topBuys[4]);
            Assert.Equal((101, 100), topBuys[5]);
            Assert.Equal((100, 101), topBuys[6]);
            
            Assert.Equal(2, topSells.Count);
            Assert.Equal((110, 200), topSells[0]);
            Assert.Equal((112, 55), topSells[1]);
        }
        
        [Fact]
        public void ZeroSizeTest()
        {
            var sut = BuildOrderBook();
            sut.Add(new AddMessage(1001, TestSymbol, 12, Side.Sell, 0, 112));
            sut.Executed(new ExecutedMessage(1001, TestSymbol, 7, Side.Buy, 100));
            sut.Update(new UpdateMessage(1001, TestSymbol, 8, Side.Buy, 0, 103));
            sut.Add(new AddMessage(1001, TestSymbol, 13, Side.Sell, 0, 113));
            sut.Update(new UpdateMessage(1001, TestSymbol, 13, Side.Sell, 10, 103));

            var topBuys = sut.GetTopBuy(Level).ToList();
            var topSells = sut.GetTopSell(Level).ToList();
            
            Assert.Equal(5, topBuys.Count);
            Assert.Equal((109, 3), topBuys[0]);
            Assert.Equal((105, 222), topBuys[1]);
            Assert.Equal((104, 44), topBuys[2]);
            Assert.Equal((101, 100), topBuys[3]);
            Assert.Equal((100, 200), topBuys[4]);
            
            Assert.Equal(3, topSells.Count);
            Assert.Equal((103, 10), topSells[0]);
            Assert.Equal((110, 200), topSells[1]);
            Assert.Equal((111, 10), topSells[2]);
        }

        private static OrderBook BuildOrderBook()
        {
            var sut = new OrderBook(TestSymbol);
            sut.Add(new AddMessage(1001, TestSymbol, 1, Side.Buy, 100, 100));
            sut.Add(new AddMessage(1002, TestSymbol, 2, Side.Sell, 100, 110));
            sut.Add(new AddMessage(1003, TestSymbol, 3, Side.Buy, 100, 100));
            sut.Add(new AddMessage(1004, TestSymbol, 4, Side.Sell, 100, 110));
            sut.Add(new AddMessage(1005, TestSymbol, 5, Side.Buy, 222, 105));
            sut.Add(new AddMessage(1006, TestSymbol, 6, Side.Sell, 10, 111));
            sut.Add(new AddMessage(1007, TestSymbol, 7, Side.Buy, 100, 102));
            sut.Add(new AddMessage(1008, TestSymbol, 8, Side.Buy, 234, 103));
            sut.Add(new AddMessage(1009, TestSymbol, 9, Side.Buy, 44, 104));
            sut.Add(new AddMessage(1010, TestSymbol, 10, Side.Buy, 100, 101));
            sut.Add(new AddMessage(1011, TestSymbol, 11, Side.Buy, 3, 109));
            
            return sut;
        }
    }
}