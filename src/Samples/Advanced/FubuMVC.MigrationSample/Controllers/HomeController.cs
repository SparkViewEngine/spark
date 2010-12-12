using JavascriptViewResultSample.Components.Paging;
using JavascriptViewResultSample.Models;
using JavascriptViewResultSample.Services.Data;
using FubuMVC.Core.Continuations;
using FubuMVC.Core;
using HtmlTags;
using Spark.Web.FubuMVC.ViewCreation;

namespace JavascriptViewResultSample.Controllers
{
    public class HomeController
    {
        public FubuContinuation Index()
        {
            return FubuContinuation.RedirectTo<HomeController>(x =>
                x.List(null));
        }

        public PagedList<RegisteredUser> List(ListInputModel input)
        {
            var pager = new Pager(input == null ? input.Page : null, 24, 6);
            PagedList<RegisteredUser> RegisteredUserList = InMemoryDataService.GetAll(pager);
            return RegisteredUserList;
        }

        public string ListPage(ListInputModel input)
        {
            var pager = new Pager(input.Page, 24, 6);
            PagedList<RegisteredUser> RegisteredUserList = InMemoryDataService.GetAll(pager);
            return JsonUtil.ToJson(RegisteredUserList);
        }

        public JavaScriptResponse ListPageView()
        {
            return new JavaScriptResponse { ViewName = "_ListPage" };
        }
    }
    
    public class ListInputModel
    {
        [QueryString]
        public int? Page { get; set; }
    }
}
