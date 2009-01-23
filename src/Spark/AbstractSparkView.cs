namespace Spark
{
    public abstract class AbstractSparkView<TExtendedContext> : SparkViewDecorator<TExtendedContext>
    {
        protected AbstractSparkView()
            : this(null)
        {
        }
        protected AbstractSparkView(SparkViewBase<TExtendedContext> decorated)
            : base(decorated)
        {
        }
    }
}
