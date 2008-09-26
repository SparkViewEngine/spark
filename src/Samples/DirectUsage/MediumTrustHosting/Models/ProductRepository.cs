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

namespace MediumTrustHosting.Models
{
    public class ProductRepository
    {
        public IList<Product> ListAll()
        {
            return new[]
                       {
                           new Product {Id = 1, Name = "Orange"},
                           new Product {Id = 2, Name = "Apple"},
                           new Product {Id = 3, Name = "Banana"}
                       };
        }

        public Product Fetch(int id)
        {
            return ListAll().FirstOrDefault(p => p.Id == id);
        }
    }
}