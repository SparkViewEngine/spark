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
using System.Collections.Generic;

namespace WindsorInversionOfControl.Models
{
    public class Menu
    {
        public string Name { get; set; }
        public IList<MenuItem> Items { get; set; }
    }

    public class MenuItem
    {
        public string Caption { get; set; }
        public string Url { get; set; }
    }

    public interface INavRepository
    {
        Menu GetMenu(string name);
    }

    public class NavRepository : INavRepository
    {
        #region INavRepository Members

        public Menu GetMenu(string name)
        {
            return new Menu
                       {
                           Name = name,
                           Items = new List<MenuItem>
                                       {
                                           new MenuItem {Caption = "Alpha", Url = "http://alpha.com"},
                                           new MenuItem {Caption = "Beta", Url = "http://beta.com"},
                                           new MenuItem {Caption = "Gamma", Url = "http://gamma.com"}
                                       }
                       };
        }

        #endregion
    }
}