using System;

namespace optimus.delivery
{
    public class Client {
        public int id {get; set;}
        public string name {get; set;}
        public string phone {get; set;}
        public string address {get; set;}
        public int addr_number {get; set;}
        public string addr_complement {get; set;}
        public DateTimeOffset created_at {get; set;}
        public DateTimeOffset updated_at {get; set;}
    }
}