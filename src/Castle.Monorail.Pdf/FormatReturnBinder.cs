using System;

namespace Castle.MonoRail.Framework
{
    [AttributeUsage(AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
    public class FormatReturnBinderAttribute : Attribute, IReturnBinder
    {
        public void Bind(IEngineContext context, IController controller, IControllerContext controllerContext, Type returnType, object returnValue)
        {
            SelectBinder(context).Bind(context, controller, controllerContext, returnType, returnValue);
        }

        private IReturnBinder SelectBinder(IEngineContext context)
        {
            //TODO: we can find a better way to do this....
            if (context.UrlInfo.Extension.ToLowerInvariant() == "json")
                return new JSONReturnBinderAttribute();
            if (context.UrlInfo.Extension.ToLowerInvariant() == "pdf")
                return new PdfReturnBinderAttribute();
            return new NullReturnBinder();
        }
    }
}