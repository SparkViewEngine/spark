using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SparkLanguagePackageLib;
using System.Runtime.InteropServices;

namespace SparkLanguage
{

    public class LanguageSupervisor : ILanguageSupervisor
    {
        public void OnSourceAssociated(ISparkSource pSource)
        {
            var sourceSupervisor = new SourceSupervisor(pSource);
            pSource.SetSupervisor(sourceSupervisor);
        }
    }
}
