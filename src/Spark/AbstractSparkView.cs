namespace Spark
{
    public abstract class AbstractSparkView : SparkViewDecorator<object>
    {
        protected AbstractSparkView()
            : this(null)
        {
        }
        protected AbstractSparkView(SparkViewBase<object> decorated)
            : base(decorated)
        {
        }
    }
}
