using System;
using System.Collections.Generic;

namespace SparkSense.Parsing
{
    public interface IViewExplorer
    {
        IList<string> GetRelatedPartials();
        IList<string> GetLocalVariables();
    }
}
