' Note: For instructions on enabling IIS6 or IIS7 classic mode, 
' visit http://go.microsoft.com/?LinkId=9394802
Imports Spark.Web.Mvc

Public Class MvcApplication
    Inherits System.Web.HttpApplication

    Shared Sub RegisterRoutes(ByVal routes As RouteCollection)
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}")

        ' MapRoute takes the following parameters, in order:
        ' (1) Route name
        ' (2) URL with parameters
        ' (3) Parameter defaults
        routes.MapRoute( _
            "Default", _
            "{controller}/{action}/{id}", _
            New With {.controller = "Home", .action = "Index", .id = ""} _
        )

    End Sub

    Shared Sub RegisterViewEngines(ByVal engines As ViewEngineCollection)
        SparkEngineStarter.RegisterViewEngine()

    End Sub



    Sub Application_Start()
        RegisterRoutes(RouteTable.Routes)
        RegisterViewEngines(ViewEngines.Engines)        
    End Sub
End Class
