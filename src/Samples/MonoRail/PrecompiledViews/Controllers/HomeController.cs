
using Castle.MonoRail.Framework;

namespace PrecompiledViews.Controllers
{
    [Layout("application")]
    public class HomeController : SmartDispatcherController
    {
        public void Index()
        {
        }

        public void List()
        {
        }

        public void Detail()
        {
        }

        public void Search()
        {
        }

        [Layout("ajax")]
        public void Notification()
        {
        }
    }
}