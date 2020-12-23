using System.Collections;
using System.Collections.Generic;
using Moq;
using Xunit;

namespace OrderBook.Tests
{
    public class OrderBookPrinterTest
    {
        private const string TestSymbol = "SYM";
        
        [Fact]
        public void InitialTest()
        {
            var sut = new OrderBookPrinter(3);
            var orderBookMock = new Mock<IOrderBook>();
            orderBookMock.Setup(_ => _.Symbol).Returns(TestSymbol);
            orderBookMock.Setup(_ => _.GetTopBuy(It.IsAny<int>())).Returns(new List<(int price, int size)>
            {
                (103, 10),
                (102, 12),
                (101, 13),
            });
            
            orderBookMock.Setup(_ => _.GetTopSell(It.IsAny<int>())).Returns(new List<(int price, int size)>
            {
                (104, 10),
                (105, 11),
                (106, 12),
            });

            var result = sut.Print(123, orderBookMock.Object);
            Assert.Equal($"123, {TestSymbol}, [(103, 10), (102, 12), (101, 13)], [(104, 10), (105, 11), (106, 12)]", result);
        }
        
        [Theory]
        [ClassData(typeof(TestData))]
        public void UpdateTest(int newSequence, 
            IList<IEnumerable<(int price, int size)>> topBuys, 
            IList<IEnumerable<(int price, int size)>> topSells, 
            string expectedResult)
        {
            var sut = new OrderBookPrinter(3);
            var printCount = 0;
            
            var orderBookMock = new Mock<IOrderBook>();
            orderBookMock.Setup(_ => _.Symbol).Returns(TestSymbol);
            orderBookMock.Setup(_ => _.GetTopBuy(It.IsAny<int>())).Returns(topBuys[printCount]);
            orderBookMock.Setup(_ => _.GetTopSell(It.IsAny<int>())).Returns(topSells[printCount]);

            sut.Print(123, orderBookMock.Object);
            printCount++;
            
            orderBookMock = new Mock<IOrderBook>();
            orderBookMock.Setup(_ => _.Symbol).Returns(TestSymbol);
            orderBookMock.Setup(_ => _.GetTopBuy(It.IsAny<int>())).Returns(topBuys[printCount]);
            orderBookMock.Setup(_ => _.GetTopSell(It.IsAny<int>())).Returns(topSells[printCount]);
            var result = sut.Print(newSequence, orderBookMock.Object);
            
            Assert.Equal(expectedResult, result);
        }

        public class TestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                // case no change in top price depths
                yield return new object[]
                {
                    124,
                    new List<IEnumerable<(int price, int size)>>
                    {
                        new List<(int price, int size)>
                        {
                            (103, 10),
                            (102, 12),
                            (101, 13),
                        },
                        new List<(int price, int size)>
                        {
                            (103, 10),
                            (102, 12),
                            (101, 13),
                        }
                    },
                    new List<IEnumerable<(int price, int size)>>
                    {
                        new List<(int price, int size)>
                        {
                            (104, 10),
                            (105, 12),
                            (106, 13),
                        },
                        new List<(int price, int size)>
                        {
                            (104, 10),
                            (105, 12),
                            (106, 13),
                        }
                    },
                    // empty string since no change
                    ""
                };
                
                // case change in top price depths - price change
                yield return new object[]
                {
                    124,
                    new List<IEnumerable<(int price, int size)>>
                    {
                        new List<(int price, int size)>
                        {
                            (103, 10),
                            (102, 12),
                            (101, 13)
                        },
                        new List<(int price, int size)>
                        {
                            (105, 10),
                            (102, 12),
                            (101, 13)
                        }
                    },
                    new List<IEnumerable<(int price, int size)>>
                    {
                        new List<(int price, int size)>
                        {
                            (102, 10),
                            (105, 12),
                            (106, 13)
                        },
                        new List<(int price, int size)>
                        {
                            (104, 10),
                            (105, 12),
                            (106, 13)
                        }
                    },
                    $"124, {TestSymbol}, [(105, 10), (102, 12), (101, 13)], [(104, 10), (105, 12), (106, 13)]"
                };
                
                // case pricedepth change in number of items
                yield return new object[]
                {
                    124,
                    new List<IEnumerable<(int price, int size)>>
                    {
                        new List<(int price, int size)>
                        {
                            (105, 10),
                            (101, 10)
                        },
                        new List<(int price, int size)>
                        {
                            (105, 10),
                            (101, 22),
                            (100, 10)
                        }
                    },
                    new List<IEnumerable<(int price, int size)>>
                    {
                        new List<(int price, int size)>
                        {
                            (107, 0)
                        },
                        new List<(int price, int size)>
                        {
                            (107, 10),
                            (109, 100)
                        }
                    },
                    $"124, {TestSymbol}, [(105, 10), (101, 22), (100, 10)], [(107, 10), (109, 100)]"
                };
                
                // case pricedepth change in number of items
                yield return new object[]
                {
                    124,
                    new List<IEnumerable<(int price, int size)>>
                    {
                        new List<(int price, int size)>
                        {
                            (105, 10),
                            (101, 10)
                        },
                        new List<(int price, int size)>
                        {
                        }
                    },
                    new List<IEnumerable<(int price, int size)>>
                    {
                        new List<(int price, int size)>
                        {
                        },
                        new List<(int price, int size)>
                        {
                            (107, 10),
                            (109, 100)
                        }
                    },
                    $"124, {TestSymbol}, [], [(107, 10), (109, 100)]"
                };
                
                // case pricedepth stays empty (no change)
                yield return new object[]
                {
                    124,
                    new List<IEnumerable<(int price, int size)>>
                    {
                        new List<(int price, int size)>
                        {
                        },
                        new List<(int price, int size)>
                        {
                        }
                    },
                    new List<IEnumerable<(int price, int size)>>
                    {
                        new List<(int price, int size)>
                        {
                        },
                        new List<(int price, int size)>
                        {
                        }
                    },
                    ""
                };
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}