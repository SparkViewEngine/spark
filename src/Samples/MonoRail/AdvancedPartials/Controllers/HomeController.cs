using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using AdvancedPartials.Models;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;

namespace AdvancedPartials.Controllers
{
    [Layout("default")]
    public class HomeController : SmartDispatcherController
    {
        public void Index()
        {
        }

        public void About()
        {
        }

        public void AddingAtPlaceholders()
        {
            
        }

        public void StyleGuide()
        {
            
        }

        public void Boxes()
        {
            
        }

        public void PagingAndRepeater(int id)
        {
            var repos = new BirdRepository();
            var birds = repos.GetBirds();

            var items = 
                PaginationHelper.CreatePagination(
                    birds, // list
                    10, // number of items per page
                    id
                );
            
            PropertyBag["items"] = items;
        }
    }
}
