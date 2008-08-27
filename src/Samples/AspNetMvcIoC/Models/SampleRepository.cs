using System.Collections.Generic;
using System.Linq;
using WindsorInversionOfControl.Models;

namespace WindsorInversionOfControl.Models
{
    public interface ISampleRepository
    {
        IList<Product> GetProducts();
    }

    public class SampleRepository : ISampleRepository
    {
        public SampleRepository()
        {
            HideProductIds = new List<int>();
        }

        public IList<int> HideProductIds { get; set; }

        public IList<Product> GetProducts()
        {
            var products = new[] {
                                     new Product {Id = 1, Name = "Apples"},
                                     new Product {Id = 2, Name = "Oranges"},
                                     new Product {Id = 3, Name = "Bananas"},
                                     new Product {Id = 4, Name = "Pineapples"},
                                     new Product {Id = 5, Name = "Puppies"},
                                     new Product {Id = 6, Name = "Mongoose"},
                                     new Product {Id = 7, Name = "Ponies"},
                                     new Product {Id = 8, Name = "Monkeys"}
                                 };


            return products.Where(prod => !HideProductIds.Contains(prod.Id)).ToList();
        }
    }
}