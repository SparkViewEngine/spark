using JavascriptViewResultSample.Components.Paging;
using JavascriptViewResultSample.Models;
using JavascriptViewResultSample.Services.Data;
using FubuMVC.Core.Continuations;
using FubuMVC.Core;
using HtmlTags;
using Spark.Web.FubuMVC.ViewCreation;

namespace JavascriptViewResultSample.Controllers
{
    public class ListViewModel
    {
        public PagedList<RegisteredUser> UserList { get; set; }
    }
    public class ListInputModel
    {
        //[QueryString]
        public int? Page { get; set; }
    }
    public class HomeController
    {
        public FubuContinuation Index()
        {
            return FubuContinuation.RedirectTo<HomeController>(x =>
                x.List(new ListInputModel { Page = 1 }));
        }

        public ListViewModel List(ListInputModel input)
        {
            var pager = new Pager(input.Page, 24, 6);
            PagedList<RegisteredUser> RegisteredUserList = InMemoryDataService.GetAll(pager);
            return new ListViewModel { UserList = RegisteredUserList };
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
}
