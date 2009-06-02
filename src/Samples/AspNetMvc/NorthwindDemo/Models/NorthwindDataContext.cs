using System;
using System.Data.Linq;
using System.Linq;

using System.Collections.Generic;

namespace NorthwindDemo.Models
{
    public partial class NorthwindDataContext
    {
        public NorthwindDataContext(IList<Category> categories)
            : base(global::System.Configuration.ConfigurationManager.ConnectionStrings["NORTHWNDConnectionString"].ConnectionString, mappingSource)
        {
            this.categories = categories;
        }

        public NorthwindDataContext(IList<Category> categories, string connectionString)
            : base(connectionString, mappingSource)
        {
            this.categories = categories;
        }

        IList<Category> categories;

        public virtual IList<Category> GetCategories()
        {
            if (this.categories == null)
                return this.Categories.ToList();
            else
                return this.categories;
        }
    }
}
