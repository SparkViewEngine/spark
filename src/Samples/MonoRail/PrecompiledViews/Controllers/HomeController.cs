
using Castle.MonoRail.Framework;
using Spark;

namespace PrecompiledViews.Controllers
{
    [Layout("application")]
    [Precompile]
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