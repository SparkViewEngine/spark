/// <reference path='jquery-1.4.1-vsdoc.js' />

function initScrollables(listPageUrl, scrollableId, visibleColumns, totalPages) {
    $("div.scrollable" + scrollableId).scrollable({
        size: visibleColumns
    });
    var url = listPageUrl + "?page=";
    var scrollable = $("div.scrollable").scrollable();

    //Seek
    scrollable.onBeforeSeek(function() {
        $("span#currentPage").fadeTo(200, 0);
    });
    scrollable.onSeek(function() {
        $("span#currentPage").html(scrollable.getPageIndex() + 1).fadeTo(200, 1);
    });

    //Click
    $("div a.load-more").click(function() {
        if (isLastPage(scrollable)) {
            var pageToGet = scrollable.getPageAmount() + 1;
            if (pageToGet <= totalPages)
                getAnotherPage(url, pageToGet, scrollable);
        }
        else
            scrollable.nextPage();
    });
}

function isLastPage(scrollable) {
    return scrollable.getPageIndex() == scrollable.getPageAmount() - 1;
}

function getAnotherPage(url, pageToGet, scrollable) {
    $("div#headerLoading").html('&nbsp;<img src="/Content/images/loading.gif"/>');
    $.getJSON(url + pageToGet, {}, function(data) {
        var contents = Spark.Home._ListPage.RenderView({ list: data });
        $("div#headerLoading").html("loaded");
        scrollable.getItemWrap().append(contents);
        scrollable.reload().end();
    });
}