using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryModdingHelper.Models
{
    public class SavedSettings
    {
        /// <summary>
        /// Plugin Reference Name. eg: LoadBalancers
        /// </summary>
        public string? CurrentPlugin { get; set; }

        /// <summary>
        /// Path to folder container .uproject file
        /// "F:\\SatisfactoryModMaking\\SML-master"
        /// </summary>
        public string? UProjectFolderPath { get; set; }

        /// <summary>
        /// Path to FactoryGame.uproject file
        /// "F:\\SatisfactoryModMaking\\SML-master\\FactoryGame.uproject"
        /// </summary>
        public string? UProjectFilePath { get; set; }

        /// <summary>
        /// Path to game folder
        /// "F:\\Games\\SteamLibrary\\steamapps\\common\\Satisfactory"
        /// </summary>
        public string? SatisfactoryFolderPath { get; set; }

        /// <summary>
        /// Path to FactoryGame.exe
        /// "F:\\Games\\SteamLibrary\\steamapps\\common\\Satisfactory\\FactoryGame.exe"
        /// </summary>
        public string? SatisfactoryExecutableFilePath { get; set; }

        /// <summary>
        /// Path to base Unreal Engine - CSS folder
        /// "C:\\Program Files\\Unreal Engine - CSS"
        /// </summary>
        public string? UnrealEngineFolderPath { get; set; }

        /// <summary>
        /// Path to Unreal Build Tool exe
        /// "C:\\Program Files\\Unreal Engine - CSS\\Engine\\Binaries\\DotNET\\UnrealBuildTool.exe"
        /// </summary>
        public string? UnrealBuildToolFilePath { get; set; }

        /// <summary>
        /// Path to Mod Manager folder
        /// "C:\\Program Files\\Satisfactory Mod Manager"
        /// </summary>
        public string? ModManagerFolderPath { get; set; }

        /// <summary>
        /// Path to Mod Manager exe
        /// "C:\\Program Files\\Satisfactory Mod Manager\\Satisfactory Mod Manager.exe"
        /// </summary>
        public string? ModManagerFilePath { get; set; }
        public bool AlpakitCloseGame { get; set; }
        public bool AlpakitCopyModToGame { get; set; }
        public bool CopyCPPFilesAfterBuild { get; set; }
        public string? Player1Name { get; set; }
        public string? Player2Name { get; set; }
        public string? Player1Args { get; set; }
        public string? Player2Args { get; set; }
        public string? Player2SatisfactoryPath { get; set; }
        public string? SinglePlayerArgs { get; set; }
        public double? AppHeight { get; set; }
        public double? AppWidth { get; set; }
        public bool ShowNotifications { get; set; }
    }
}
