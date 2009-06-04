$(function(){

$(".refresh").click(function(e) {
    e.preventDefault();
    $.getJSON('/Home/RefreshCart', function(data) {
        var contents = Spark.Home._ShowCart.RenderView({cart:data});
        $('#cart').html(contents);
      });
    });

$(".cartajax").livequery(
  function(){
    $(this).click(function(e) {
      e.preventDefault();
      $.post($(this).attr('href'), {}, function(data) {
        var contents = Spark.Home._ShowCart.RenderView({cart:data});
        $('#cart').html(contents);
      }, "json");
    });
  });
  
});

function SiteResource(path) {
    if (path.length >= 2 && path.substr(0, 2) == '~/')
        return appBasePath + path.substr(2);
    return path;
}

window.Html = 
{
    FormatPrice:function(value) {return value.toFixed(2);}
};
