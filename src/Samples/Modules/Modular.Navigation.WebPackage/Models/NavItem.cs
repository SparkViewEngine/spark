namespace Modular.Navigation.WebPackage.Models
{
    public class NavItem
    {
        public int Weight { get; set; }
        public string Caption { get; set; }
        public string Action { get; set; }
        public object LinkValues { get; set; }
    }
}