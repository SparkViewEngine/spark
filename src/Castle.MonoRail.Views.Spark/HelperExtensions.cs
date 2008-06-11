using Castle.MonoRail.Framework.Helpers;

namespace Castle.MonoRail.Views.Spark
{
	public static class HelperExtensions
	{
		public static string AjaxFormTag(this FormHelper helper, object parameters)
		{ return helper.AjaxFormTag(new ModelDictionary(parameters)); }

		public static string FormTag(this FormHelper helper, object parameters)
		{ return helper.FormTag(new ModelDictionary(parameters)); }

		public static string FormTag(this FormHelper helper, string url, object parameters)
		{ return helper.FormTag(url, new ModelDictionary(parameters)); }

		public static string TextField(this FormHelper helper, string target, object attributes)
		{ return helper.TextField(target, new ModelDictionary(attributes)); }


		public static string For(this UrlHelper helper, object parameters)
		{ return helper.For(new ModelDictionary(parameters)); }
	}
}