using System;
using System.Collections.Generic;


namespace NorthwindDemo.Models
{
    public class ProductsEditViewData
    {
        public Product Product           { get; set; }
        public List<Supplier> Suppliers  { get; set; }
        public List<Category> Categories { get; set; }
    }

    public class ProductsNewViewData
    {
        public List<Supplier> Suppliers  { get; set; }
        public List<Category> Categories { get; set; }
    } 
}
