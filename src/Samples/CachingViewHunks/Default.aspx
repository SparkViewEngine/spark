<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Assembly Name="System.Web.Mvc, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" %>
<%@ Assembly Name="System.Web.Routing, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        // Change the current path so that the Routing handler can correctly interpret
        // the request, then restore the original path so that the OutputCache module
        // can correctly process the response (if caching is enabled).

        var originalPath = Request.Path;
        HttpContext.Current.RewritePath(Request.ApplicationPath, false);
        IHttpHandler httpHandler = new System.Web.Mvc.MvcHttpHandler();
        httpHandler.ProcessRequest(HttpContext.Current);
        HttpContext.Current.RewritePath(originalPath, false);
    }
</script>

