using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Spark.Compiler
{
    public static class AssemblyExtensions
    {
        public static bool IsDynamic(this Assembly assembly)
        {
#if NETFRAMEWORK || NET
            if (assembly is AssemblyBuilder)
            {
                return true;
            }
#endif

#if NETFRAMEWORK
            if (assembly.ManifestModule.GetType().Namespace == "System.Reflection.Emit" /* .Net 4 specific */)
            {
                return true;
            }
#endif
            
            return assembly.HasNoLocation();
        }

        private static bool HasNoLocation(this Assembly assembly)
        {
            bool result;

            try
            {
                result = string.IsNullOrEmpty(assembly.Location);
            }
            catch (NotSupportedException)
            {
                return true;
            }

            return result;
        }
    }
}