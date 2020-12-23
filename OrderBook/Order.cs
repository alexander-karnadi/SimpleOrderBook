namespace OrderBook
{
    public sealed class Order
    {
        public int Id { get; }
        public int Price { get; set; }
        public int Size { get; set; }

        public Order(int id, int price, int size)
        {
            Id = id;
            Price = price;
            Size = size;
        }
    }
}