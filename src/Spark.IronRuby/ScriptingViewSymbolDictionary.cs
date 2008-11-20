using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace Spark.IronRuby
{
    public class ScriptingViewSymbolDictionary : CustomSymbolDictionary
    {
        private readonly IScriptingSparkView _view;
        private readonly Type _viewType;

        private static readonly SymbolId _viewSymbol = SymbolTable.StringToId("view");

        public ScriptingViewSymbolDictionary(IScriptingSparkView view)
        {
            _view = view;
            _viewType = view.GetType();
        }

        public override SymbolId[] GetExtraKeys()
        {
            throw new System.NotImplementedException();
        }

        protected override bool TrySetExtraValue(SymbolId key, object value)
        {
            var property = _viewType.GetProperty(key.ToString());
            if (property != null)
            {
                property.SetValue(_view, value, null);
                return true;
            }

            var field = _viewType.GetField(key.ToString());
            if (field != null)
            {
                field.SetValue(_view, value);
                return true;
            }

            return false;
        }

        protected override bool TryGetExtraValue(SymbolId key, out object value)
        {
            if (key == _viewSymbol)
            {
                value = _view;
                return true;
            }

            var property = _viewType.GetProperty(key.ToString());
            if (property != null)
            {
                value = property.GetValue(_view, null);
                return true;
            }

            var field = _viewType.GetField(key.ToString());
            if (field != null)
            {
                value = field.GetValue(_view);
                return true;
            }

            if (_view.TryGetViewData(key.ToString(), out value))
                return true;

            value = null;
            return false;
        }
    }
}