using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FubuCore.Binding;
using JavascriptViewResultSample.Components.Paging;

namespace FubuMVC.MigrationSample.Components.Paging
{
    public class PagedListModelBinder<T> : IModelBinder where T : class
    {
        public object Bind(Type type, IBindingContext context)
        {
            object instance = new PagedList<T>(new Pager(), new List<T>(), 0);
            Bind(type, instance, context);
            return instance;
        }

        public void Bind(Type type, object instance, IBindingContext context)
        {
            context.StartObject(instance);

            var pagedList = instance as PagedList<T>;
            //pagedList.pop

            context.FinishObject();
        }

        public bool Matches(Type type)
        {
            return false;// type == typeof(PagedList<T>);
        }
    }
}