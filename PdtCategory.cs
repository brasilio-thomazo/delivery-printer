using System;

namespace optimus.delivery
{
    public class PdtCategory {
        public int id {get; set;}
        public string name {get; set;}
        public DateTimeOffset created_at {get; set;}
        public DateTimeOffset updated_at {get; set;}
    }
}