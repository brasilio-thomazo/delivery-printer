using System;

namespace optimus.delivery
{
    public class OrderItemPart
    {
        public int id { get; set; }
        public int id_order_item { get; set; }
        public int id_product { get; set; }
        public float cost { get; set; }
        public float price { get; set; }
        public Product product { get; set; }
        public DateTimeOffset created_at { get; set; }
        public DateTimeOffset updated_at { get; set; }
    }
}