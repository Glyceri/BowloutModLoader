namespace BowloutModLoader.Utils
{
    public static class PlatformChecker
    {
        public static Platform CurrentPlatform()
        {
            PlatformID currentPlatform = Environment.OSVersion.Platform;
            if (currentPlatform == PlatformID.Unix) return Platform.Linux;
            if (currentPlatform == PlatformID.Win32NT) return Platform.Windows;
            return Platform.MacOS;
        }

        public static bool IsWindows() => CurrentPlatform() == Platform.Windows;
        public static bool IsLinux() => CurrentPlatform() == Platform.Linux;
        public static bool IsMacOS() => CurrentPlatform() == Platform.MacOS;
    }
}
