using System;

namespace optimus.delivery
{
    public class Product {
        public int id {get; set;}
        public string name {get; set;}
        public int id_type {get; set;}
        public int id_category {get; set;}
        public string description {get; set;}
        public float cost {get; set;}
        public float price {get; set;}
        public DateTimeOffset created_at {get; set;}
        public DateTimeOffset updated_at {get; set;}
        public PdtType type {get; set;}
        public PdtCategory category {get; set;}
    }
}