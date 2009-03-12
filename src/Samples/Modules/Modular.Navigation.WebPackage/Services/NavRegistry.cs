using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modular.Navigation.WebPackage.Models;
using Spark.Modules;

namespace Modular.Navigation.WebPackage.Services
{
    public interface INavRegistry : IService
    {
        void AddItem(NavItem item);
        IEnumerable<NavItem> ListItems();
    }

    public class NavRegistry : INavRegistry
    {
        private IEnumerable<NavItem> _items = new NavItem[0];

        public void AddItem(NavItem item)
        {
            _items = _items.Union(new[] {item}).OrderBy(i => i.Weight).ToArray();
        }

        public IEnumerable<NavItem> ListItems()
        {
            return _items;
        }
    }

}
