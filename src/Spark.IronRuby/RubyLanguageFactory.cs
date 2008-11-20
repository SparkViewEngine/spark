using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Spark.Compiler;
using Spark.IronRuby.Compiler.Ruby;

namespace Spark.IronRuby
{
    public class RubyLanguageFactory : DefaultLanguageFactory
    {

        private RubyEngineManager _RubyEngineManager;
        public RubyEngineManager RubyEngineManager
        {
            get
            {
                if (_RubyEngineManager == null)
                    Interlocked.CompareExchange(ref _RubyEngineManager, new RubyEngineManager(), null);
                return _RubyEngineManager;
            }
        }

        public override Spark.Compiler.ViewCompiler CreateViewCompiler(ISparkViewEngine engine, SparkViewDescriptor descriptor)
        {
            ViewCompiler viewCompiler;
            switch (descriptor.Language)
            {
                case LanguageType.Default:
                case LanguageType.Ruby:
                    viewCompiler = new RubyViewCompiler();
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
            if (compiler is RubyViewCompiler && view is IScriptingSparkView)
            {
                RubyEngineManager.InstanceCreated(
                    (RubyViewCompiler)compiler,
                    (IScriptingSparkView)view);
            }
            else
            {
                base.InstanceCreated(compiler, view);
            }
        }

        public override void InstanceReleased(ViewCompiler compiler, ISparkView view)
        {
            if (compiler is RubyViewCompiler && view is IScriptingSparkView)
            {
                RubyEngineManager.InstanceReleased(
                    (RubyViewCompiler)compiler,
                    (IScriptingSparkView)view);
            }
            else
            {
                base.InstanceCreated(compiler, view);
            }
        }
    }
}
