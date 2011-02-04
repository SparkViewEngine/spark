using System.Dynamic;
using System.Web.Mvc;

namespace Spark.Web.Mvc
{
	public class DynamicViewDataDictionary : DynamicObject
	{
		private readonly ViewDataDictionary _viewData;

		public DynamicViewDataDictionary( ViewDataDictionary viewData )
		{
			_viewData = viewData;
		}

		public override bool TryGetMember( GetMemberBinder binder, out object result )
		{
			result = _viewData[binder.Name];
			return true;
		}

		public override bool TrySetMember( SetMemberBinder binder, object value )
		{
			_viewData[binder.Name] = value;
			return true;
		}
	}
}
