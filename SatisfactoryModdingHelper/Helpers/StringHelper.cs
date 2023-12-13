using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryModdingHelper.Helpers
{
    //Helper class for common strings and such
    public static class StringHelper
    {
        public const string RunningAlpakit = "Running Alpakit...";
        public const string AlpakitComplete = "Alpakit Complete";
        public const string LaunchingMPHost = "Launching MP Host";
        public const string LaunchingMPClient = "Launching MP Client";
        public const string RunningMirror = "Mirroring Satisfactory install to secondary location...";
        public const string MirrorComplete = "Mirroring Complete";
        public const string WinShipping = "FactoryGame Win64 Shipping";
        public const string WinDev = "FactoryGameEditor Win64 Development";
        public const string BuildingDev = "Building Development Editor...";
        public const string BuildForDev = "Build for Development Editor";
        public const string BuildDevComplete = "Build for Development Editor Complete";
        public const string BuildingShipping = "Building Shipping...";
        public const string BuildForShipping = "Build for Shipping";
        public const string BuildShippingComplete = "Build for Shipping Complete";
        public const string LaunchingSatisfactory = "Launching Satisfactory...";
        public const string LaunchingSMM = "Launching Satisfactory Mod Manager...";

        public static string GetAlpakitArgs(bool shouldCopyMod, string? satisfactoryFolderPath, string? uprojectFilePath, string? modName)
        {
            var alpakitArgs = shouldCopyMod ? $"-CopyToGameDir -GameDir=\"{satisfactoryFolderPath}\"" : "";
            var args = $" -ScriptsForProject=\"{uprojectFilePath}\" PackagePlugin -Project=\"{uprojectFilePath}\" -DLCName=\"{modName}\" -ClientConfig=Shipping -ServerConfig=Shipping -Platform=Win64 -NoCompileEditor -Installed {alpakitArgs}";
            return args;
        }

        public static string GetUATBatPath(string? unrealEngineFolderPath)
        {
            return @$"{unrealEngineFolderPath}\Engine\Build\BatchFiles\RunUAT.bat";
        }

        public static string GetMPLaunchArgs(string? playerArgs)
        {
            return $"-EpicPortal -NoSteamClient {playerArgs}";
        }

        public static string GetRobocopyArgs(string? satisfactoryFolderPath, string? newLocation)
        {
            return $"\"{satisfactoryFolderPath}\" \"{newLocation}\" /PURGE /MIR /XD Configs /R:2 /W:2 /NS /NDL /NFL /NP";
        }

        public static string GetBuildBatPath(string? unrealEngineFolderPath)
        {
            return $"\"{unrealEngineFolderPath}\\Engine\\Build\\BatchFiles\\Build.bat\"";
        }

        public static string GetBuildArgs(string? environmentToBuild, string? uprojectFilePath)
        {
            return $"{environmentToBuild} -Project=\"{uprojectFilePath}\" -WaitMutex -FromMsBuild";
        }
            
    }
}
