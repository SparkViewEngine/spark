using WindsorInversionOfControl.Models;

namespace WindsorInversionOfControl.Views
{
    public abstract class View<T> : MvcContrib.SparkViewEngine.SparkView<T> where T : class
    {
        public INavRepository Nav { get; set; }

        public string Yadda()
        {
            return "xx";
        }
    }

    public abstract class View : View<object>
    {
        
    }
}