namespace App.Models.Theme
{
    public static class ThemeSettings
    {
        public const string Light = "light";
        public const string Dark = "dark";
        public const string System = "system";

        public static readonly string[] AvailableThemes = new[]
        {
            Light,
            Dark,
            System
        };

        public static bool IsValidTheme(string theme)
        {
            return theme != null && AvailableThemes.Contains(theme.ToLower());
        }

        public static string GetDefaultTheme()
        {
            return Light;
        }
    }
} 