Partial Public Class _Default
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Change the current path so that the Routing handler can correctly interpret
        ' the request, then restore the original path so that the OutputCache module
        ' can correctly process the response (if caching is enabled).

        Dim originalPath As String = Request.Path
        HttpContext.Current.RewritePath(Request.ApplicationPath, False)
        Dim httpHandler As IHttpHandler = New MvcHttpHandler()
        httpHandler.ProcessRequest(HttpContext.Current)
        HttpContext.Current.RewritePath(originalPath, False)
    End Sub

End Class
