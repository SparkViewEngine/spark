using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.MonoRail.Framework;
using ViewComponents.Models;

namespace ViewComponents.ViewComponents
{
    [ViewComponentDetails("MailingAddress")]
    public class MailingAddress : ViewComponent
    {
        [ViewComponentParam]
        public string Caption { get; set; }

        [ViewComponentParam]
        public Account Address { get; set; }

        public override void Render()
        {
            PropertyBag["Caption"] = Caption;
            PropertyBag["Address"] = Address;

            RenderView("default");
        }
    }
}
