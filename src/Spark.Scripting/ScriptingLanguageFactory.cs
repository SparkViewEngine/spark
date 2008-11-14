using System;
using System.Linq;
using System.Text;
using System.Threading;
using Spark.Compiler;
using Spark.Compiler.Javascript;
using Spark.Scripting.Compiler.Python;

namespace Spark.Scripting
{
    public class ScriptingLanguageFactory : DefaultLanguageFactory
    {
        private PythonEngineManager _PythonEngineManager;
        public PythonEngineManager PythonEngineManager
        {
            get
            {
                if (_PythonEngineManager == null)
                    Interlocked.CompareExchange(ref _PythonEngineManager, new PythonEngineManager(), null);
                return _PythonEngineManager;
            }
        }

        public override ViewCompiler CreateViewCompiler(ISparkViewEngine engine, SparkViewDescriptor descriptor)
        {
            ViewCompiler viewCompiler;
            switch (descriptor.Language)
            {
                case LanguageType.Default:
                case LanguageType.Python:
                    viewCompiler = new PythonViewCompiler();
                    break;
                default:
                    return base.CreateViewCompiler(engine, descriptor);
            }

            var pageBaseType = engine.Settings.PageBaseType;
            if (string.IsNullOrEmpty(pageBaseType))
                pageBaseType = engine.DefaultPageBaseType;

            viewCompiler.BaseClass = pageBaseType;
            viewCompiler.Descriptor = descriptor;
            viewCompiler.Debug = engine.Settings.Debug;
            viewCompiler.UseAssemblies = engine.Settings.UseAssemblies;
            viewCompiler.UseNamespaces = engine.Settings.UseNamespaces;
            return viewCompiler;
        }

        public override void InstanceCreated(ViewCompiler compiler, ISparkView view)
        {
            if (compiler is PythonViewCompiler && view is IScriptingSparkView)
            {
                PythonEngineManager.InstanceCreated(
                    (PythonViewCompiler) compiler,
                    (IScriptingSparkView) view);
            }
            else
            {
                base.InstanceCreated(compiler, view);
            }
        }

        public override void InstanceReleased(ViewCompiler compiler, ISparkView view)
        {
            if (compiler is PythonViewCompiler && view is IScriptingSparkView)
            {
                PythonEngineManager.InstanceReleased(
                    (PythonViewCompiler)compiler,                                                    
                    (IScriptingSparkView)view);
            }
            else
            {
                base.InstanceCreated(compiler, view);
            }
        }
    }
}
