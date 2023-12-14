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
        public const string GenVSFiles = "Generating VS Files...";
        public const string GenVSFilesComplete = "Generate VS Files Complete";
        public const string BuildcsTemplateName = "Buildcs.txt";
        public const string ModulehTemplateName = "Module.h.txt";
        public const string ModulecppTemplateName = "Module.cpp.txt";
        public const string BPFLhTemplateName = "BPFL.h.txt";
        public const string BPFLcppTemplateName = "BPFL.cpp.txt";
        public const string RCOhTemplateName = "RCO.h.txt";
        public const string RCOcppTemplateName = "RCO.cpp.txt";
        public const string SubsystemhTemplateName = "Subsystem.h.txt";
        public const string SubsystemcppTemplateName = "Subsystem.cpp.txt";
        public const string TemplateModReferencePlaceholder = "[ModReference]";
        public const string TemplateModReferenceUCPlaceholder = "[ModReferenceUC]";
        public const string Buildcs = "Build.cs";
        public const string Moduleh = "Module.h";
        public const string Modulecpp = "Module.cpp";

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

        public static string GetGenerateVSFilesArgs(string? uProjectFilePath)
        {
            return $"-projectfiles -project=\"{uProjectFilePath}\" -game -rocket -progress";
        }

        public static string GetModPublicFolderPath(string? modFolderPath, string? modName)
        {
            return $"{modFolderPath}//Source//{modName}//Public";
        }

        public static string GetModPrivateFolderPath(string? modFolderPath, string? modName)
        {
            return $"{modFolderPath}//Source//{modName}//Private";
        }
        public static string GetModSourceFolderPath(string? modFolderPath, string? modName)
        {
            return $"{modFolderPath}//Source//{modName}";
        }

        public static string GetModFolderPath(string? uProjectFolderPath, string? modName)
        {
            return $"{uProjectFolderPath}//Mods//{modName}";
        }

        public static string GetBuildcsFilePath(string? modFolderPath, string? modName)
        {
            return $"{modFolderPath}//Source//{modName}//{modName}.{Buildcs}";
        }
        public static string GetModulehFilePath(string? modFolderPath, string? modName)
        {
            return $"{modFolderPath}//Source//{modName}//{modName}{Moduleh}";
        }
        public static string GetModulecppFilePath(string? modFolderPath, string? modName)
        {
            return $"{modFolderPath}//Source//{modName}//{modName}{Modulecpp}";
        }

        public static string GetUpluginFilePath(string? modFolderPath, string? modName)
        {
            return $"{modFolderPath}//{modName}.uplugin";
        }
    }
}
