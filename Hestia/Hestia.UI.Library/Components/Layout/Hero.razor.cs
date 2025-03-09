namespace Hestia.UI.Library.Components.Layout
{
    public enum HeroSize
    {
        Default,
        Small,
        Medium,
        Large,
        HalfHeight,
        FullHeight
    }
    public static class HeroSizeExtensions
    {
        public static string Class(this HeroSize size)
        {
            return size switch
            {
                HeroSize.Small => "is-small",
                HeroSize.Medium => "is-medium",
                HeroSize.Large => "is-large",
                HeroSize.HalfHeight => "is-halfheight",
                HeroSize.FullHeight => "is-fullheight",
                _ => "",
            };
        }
    }

}
