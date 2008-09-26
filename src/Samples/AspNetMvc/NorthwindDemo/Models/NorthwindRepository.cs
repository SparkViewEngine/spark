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
using System.Linq;

namespace NorthwindDemo.Models
{
    public class NorthwindRepository
    {
        private readonly NorthwindDataContext dataContext;

        public NorthwindRepository()
        {
        }

        public NorthwindRepository(NorthwindDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public virtual IQueryable<Category> Categories
        {
            get { return dataContext.Categories; }
        }

        public virtual IQueryable<Product> Products
        {
            get { return dataContext.Products; }
        }

        public virtual IQueryable<Supplier> Suppliers
        {
            get { return dataContext.Suppliers; }
        }

        public virtual void SubmitChanges()
        {
            dataContext.SubmitChanges();
        }

        public virtual void InsertProductOnSubmit(Product p)
        {
            dataContext.Products.InsertOnSubmit(p);
        }
    }
}