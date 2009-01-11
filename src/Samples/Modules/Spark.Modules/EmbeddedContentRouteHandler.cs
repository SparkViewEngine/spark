using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Routing;

namespace Spark.Modules
{
    internal class EmbeddedContentRouteHandler : IRouteHandler
    {
        private readonly Assembly _assembly;
        private readonly string _resourcePath;

        public EmbeddedContentRouteHandler(Assembly assembly, string resourcePath)
        {
            _assembly = assembly;
            _resourcePath = resourcePath;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new EmbeddedContentHttpHandler(this, requestContext);
        }

        public Stream GetStream(string resource)
        {
            return _assembly.GetManifestResourceStream(_resourcePath + "." + resource.Replace('/', '.'));
        }

        internal class EmbeddedContentHttpHandler : IHttpHandler
        {
            private readonly EmbeddedContentRouteHandler _routeHandler;
            private readonly RequestContext _requestContext;

            public EmbeddedContentHttpHandler(EmbeddedContentRouteHandler routeHandler, RequestContext requestContext)
            {
                _routeHandler = routeHandler;
                _requestContext = requestContext;
            }

            public bool IsReusable { get { return false; } }

            public void ProcessRequest(HttpContext context)
            {
                var resource = _requestContext.RouteData.GetRequiredString("resource");
                switch (Path.GetExtension(resource))
                {
                    case ".css":
                        context.Response.ContentType = "text/css";
                        break;
                    case ".js":
                        context.Response.ContentType = "application/x-javascript";
                        break;
                    case ".png":
                        context.Response.ContentType = "image/png";
                        break;
                    case ".gif":
                        context.Response.ContentType = "image/gif";
                        break;
                    case ".jpg":
                        context.Response.ContentType = "image/jpeg";
                        break;
                    case ".bmp":
                        context.Response.ContentType = "image/bmp";
                        break;
                }
                using (var stream = _routeHandler.GetStream(resource))
                {
                    var buffer = new byte[1024];
                    for (; ; )
                    {
                        var size = stream.Read(buffer, 0, buffer.Length);
                        if (size == 0)
                            break;
                        context.Response.OutputStream.Write(buffer, 0, size);
                    }
                }
            }
        }
    }
}