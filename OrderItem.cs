using System;
using System.Collections.Generic;

namespace optimus.delivery
{
    public class OrderItem {
        public int id {get; set;}
        public string name {get; set;}
        public int id_order {get; set;}
        public float cost {get; set;}
        public float price {get; set;}
        public DateTimeOffset created_at {get; set;}
        public DateTimeOffset updated_at {get; set;}

        public List<OrderItemPart> parts {get; set;}
    }
}