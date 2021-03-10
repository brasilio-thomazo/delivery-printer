using System;
using System.Collections.Generic;

namespace optimus.delivery
{
    public class Order
    {
        public int id { get; set; }
        public int id_user { get; set; }
        public int id_client { get; set; }
        public int id_payment { get; set; }
        public Payment payment { get; set; }
        public float price { get; set; }
        public float cost { get; set; }
        public float pay { get; set; }
        public float repay { get; set; }
        public string observation { get; set; }
        public DateTimeOffset created_at { get; set; }
        public DateTimeOffset updated_at { get; set; }
        public Client client { get; set; }
        public User user { get; set; }
        public List<OrderItem> items { get; set; }
    }
}