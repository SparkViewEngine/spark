using System;

namespace Castle.MonoRail.Framework
{
    internal class NullReturnBinder : IReturnBinder
    {
        public void Bind(IEngineContext context, IController controller, IControllerContext controllerContext, Type returnType, object returnValue)
        {
            //no-op
        }
    }
}