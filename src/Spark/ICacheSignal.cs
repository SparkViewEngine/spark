using System;

namespace Spark
{
    public interface ICacheSignal
    {
        event EventHandler Changed;
    }
}
