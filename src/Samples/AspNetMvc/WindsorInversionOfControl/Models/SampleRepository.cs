// Copyright 2008 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System.Collections.Generic;
using System.Linq;

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

        #region ISampleRepository Members

        public IList<Product> GetProducts()
        {
            var products = new[]
                               {
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

        #endregion
    }
}