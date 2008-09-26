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
using System.Configuration;
using System.Linq;

namespace NorthwindDemo.Models
{
    public partial class NorthwindDataContext
    {
        private readonly IList<Category> categories;

        public NorthwindDataContext(IList<Category> categories)
            : base(ConfigurationManager.ConnectionStrings["NORTHWNDConnectionString"].ConnectionString, mappingSource)
        {
            this.categories = categories;
        }

        public NorthwindDataContext(IList<Category> categories, string connectionString)
            : base(connectionString, mappingSource)
        {
            this.categories = categories;
        }

        public virtual IList<Category> GetCategories()
        {
            if (categories == null)
                return Categories.ToList();
            else
                return categories;
        }
    }
}