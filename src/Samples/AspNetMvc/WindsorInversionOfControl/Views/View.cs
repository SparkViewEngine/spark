using WindsorInversionOfControl.Models;

namespace WindsorInversionOfControl.Views
{
    /// <summary>
    /// Base class for typed spark view files
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class View<T> : Spark.Web.Mvc.SparkView<T> where T : class
    {
        public INavRepository Nav { get; set; }

        public string Yadda()
        {
            return "Adding a convenience function";
        }

        /// <summary>
        /// Throw in a for ViewData.Model
        /// </summary>
        public T Model {get { return ViewData.Model;}}
    }

    /// <summary>
    /// Untyped base for spark view files. Inherits members from above.
    /// </summary>
    public abstract class View : View<object>
    {
        
    }
}
