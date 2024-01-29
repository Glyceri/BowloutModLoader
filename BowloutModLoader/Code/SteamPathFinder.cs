using BowloutModLoader.Enums;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;
using System.IO;

namespace BowloutModLoader.Utils
{
    internal static class SteamPathFinder
    {
        const int MAX_ESTIMATED_DRIVE_COUNT = 10;

        public static string? GetPath(string gamePath)
        {
            if (!PlatformChecker.IsWindows()) return null;
            string[]? gameDirectories = GetGameDirectories();
            if (gameDirectories == null) return null;

            foreach (string gameDirectory in gameDirectories)
                if (gameDirectory.EndsWith(gamePath))
                    return gameDirectory;

            return null;
        }

        static string[]? GetGameDirectories()
        {
            string? steamInstallPath = GetSteamInstallPath();
            if (steamInstallPath == null) return null;
            string configFilePath = steamInstallPath + BaseString.libraryFolder;
            return ReadFile(configFilePath);
        }

        static string[]? ReadFile(string configFilePath)
        {
            if (!File.Exists(configFilePath)) return null;
            VProperty steamInfo = VdfConvert.Deserialize(File.ReadAllText(configFilePath));
            List<string> directories = new List<string>();
            for (int i = 0; i < MAX_ESTIMATED_DRIVE_COUNT; i++)
            {
                string activePath;
                VToken? token = steamInfo.Value[i.ToString()];
                if (token == null) break;
                activePath = token[BaseString.pathAttribute]?.ToString() ?? BaseString.errorPath;
                string currentPath = activePath + BaseString.steamApps;
                if (!Directory.Exists(currentPath)) return null;
                directories.AddRange(Directory.GetDirectories(currentPath));
            }
            return directories.ToArray();
        }
#pragma warning disable CA1416 // Validate platform compatibility (We do infact, check for the correct Platform)
        static string? GetSteamInstallPath()
        {
            if (!PlatformChecker.IsWindows()) return null;
            using (RegistryKey? softwareKey = Registry.LocalMachine.OpenSubKey(BaseString.steamRegisteryPath))
            {
                if (softwareKey == null) //Implement another way of getting steam install path
                    return null;
                return softwareKey.GetValue(BaseString.steamInstallPathKey)?.ToString() ?? BaseString.errorPath;
            }
        }
#pragma warning restore CA1416 // Validate platform compatibility
    }
}


