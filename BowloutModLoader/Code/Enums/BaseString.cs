namespace BowloutModLoader.Enums
{
    public static class BaseString
    {
        /// <summary>
        /// This empty value is equal to string.Empty
        /// </summary>
        public static readonly string empty = string.Empty;
        public static readonly string errorPath = "[NO PATH]";
        public static readonly string steamRegisteryPath = "SOFTWARE\\WOW6432Node\\Valve\\Steam";
        public static readonly string steamInstallPathKey = "InstallPath";
        public static readonly string libraryFolder = @"/steamapps/libraryfolders.vdf";
        public static readonly string steamApps = @"\steamapps\common\";
        public static readonly string pathAttribute = "path";
        public static readonly string appsAttribute = "apps";
        public static readonly string steamIdName = "steam_appid.txt";
        public static readonly string GAME_NAME = "BOWLOUT Playtest";

        public static bool IsError(this string checkForError) => checkForError == errorPath;
    }
}
