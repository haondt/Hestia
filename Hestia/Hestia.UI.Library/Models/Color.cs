namespace Hestia.UI.Library.Models
{
    public enum Color
    {
        Default,
        Link,
        Primary,
        Info,
        Success,
        Warning,
        Danger
    }

    public static class ColorExtensions
    {
        public static string Class(this Color color)
        {
            return color switch
            {
                Color.Link => "is-link",
                Color.Primary => "is-primary",
                Color.Info => "is-info",
                Color.Success => "is-success",
                Color.Warning => "is-warning",
                Color.Danger => "is-danger",
                _ => ""
            };
        }
    }
}
