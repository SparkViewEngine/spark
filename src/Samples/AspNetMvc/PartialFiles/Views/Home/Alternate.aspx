<%@ Page Language="C#" MasterPageFile="~/Views/Shared/DefaultLayout.Master" %>
<%@ Import Namespace="Microsoft.Web.Mvc"%>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="view" runat="server">
    <p>
        Top of the view</p>
    <div style="border: solid 1px grey">
        <p>
            RenderPartial Yay ascx</p>
        <% Html.RenderPartial("Yay"); %>
    </div>
    <div style="border: solid 1px grey">
        <p>
            RenderPartial Working aspx</p>
        <% Html.RenderPartial("Working"); %>
    </div>
    <div style="border: solid 1px grey">
        <p>
            RenderPartial _Status spark</p>
        <% Html.RenderPartial("_Status"); %>
    </div>
  <div style="border:solid 1px grey">
    <p>RenderAction ShowStatus</p>
    <% Html.RenderAction("ShowStatus"); %>
  </div>
    <p>
        Bottom of the view</p>
</asp:Content>

