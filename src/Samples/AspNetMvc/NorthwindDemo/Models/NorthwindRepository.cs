using System;
using System.Linq;
using System.Data.Linq;

namespace NorthwindDemo.Models
{
    public class NorthwindRepository
    {
        NorthwindDataContext dataContext = null;

        public NorthwindRepository()
        { 
        }

        public NorthwindRepository(NorthwindDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public virtual IQueryable<Category> Categories
        { 
            get
            {
                return this.dataContext.Categories;
            }
        }

        public virtual IQueryable<Product> Products
        {
            get
            {
                return this.dataContext.Products;
            }
        }

        public virtual IQueryable<Supplier> Suppliers
        {
            get 
            {
                return this.dataContext.Suppliers;    
            }
        }

        public virtual void SubmitChanges()
        {
            this.dataContext.SubmitChanges();
        }

        public virtual void InsertProductOnSubmit(Product p)
        {
            this.dataContext.Products.InsertOnSubmit(p);
        }

        public virtual void InsertCategoryOnSubmit(Category c) {
            this.dataContext.Categories.InsertOnSubmit(c);
        }
    }
}