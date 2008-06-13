/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System.Collections.Generic;
using System.Text;

namespace Spark
{
    public abstract class AbstractSparkView : ISparkView
    {
        private readonly Dictionary<string, StringBuilder> _content = new Dictionary<string, StringBuilder>();

        public Dictionary<string, StringBuilder> Content { get { return _content; } }

        protected StringBuilder BindContent(string name)
        {
            StringBuilder sb;
            if (!_content.TryGetValue(name, out sb))
            {
                sb = new StringBuilder();
                _content.Add(name, sb);
            }
            return sb;
        }

        public abstract string RenderView();
    }
}
