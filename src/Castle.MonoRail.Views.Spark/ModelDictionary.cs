using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Castle.MonoRail.Views.Spark
{
	public class ModelDictionary : Dictionary<string,object>
	{
		public ModelDictionary(object model)
			: base(StringComparer.InvariantCultureIgnoreCase)
		{
			foreach(var member in model.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty))
			{
				if (member is FieldInfo)
					Add(member.Name, (member as FieldInfo).GetValue(model));

				if (member is PropertyInfo)
					Add(member.Name, (member as PropertyInfo).GetValue(model, null));
			}
		}
	}
}
