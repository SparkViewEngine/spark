using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modular.Navigation.WebPackage.Models;
using Spark.Modules;

namespace Modular.Navigation.WebPackage.Services
{
    public interface ISideRegistry : IService
    {
        void AddItem(SideItem item);
        IEnumerable<SideItem> ListItems();
    }

    public class SideRegistry : ISideRegistry
    {
        private IEnumerable<SideItem> _items = new SideItem[0];

        public void AddItem(SideItem item)
        {
            _items = _items.Union(new[] { item }).OrderBy(i => i.Weight).ToArray();
        }

        public IEnumerable<SideItem> ListItems()
        {
            return _items;
        }
    }
}
