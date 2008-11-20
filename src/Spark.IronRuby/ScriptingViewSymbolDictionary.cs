// Copyright 2008 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace Spark.Ruby
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
            throw new NotImplementedException();
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