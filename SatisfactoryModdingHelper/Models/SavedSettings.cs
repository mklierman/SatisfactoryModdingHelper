using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryModdingHelper.Models
{
    public class SavedSettings
    {
        public string CurrentPlugin { get; set; }
        public string UProjectFolderPath { get; set; }
        public string UProjectFilePath { get; set; }
        public string SatisfactoryFolderPath { get; set; }
        public string SatisfactoryExecutableFilePath { get; set; }
        public string UnrealEngineFolderPath { get; set; }
        public string UnrealBuildToolFilePath { get; set; }
        public string ModManagerFolderPath { get; set; }
        public string ModManagerFilePath { get; set; }
        public bool AlpakitCloseGame { get; set; }
        public bool AlpakitCopyModToGame { get; set; }
        public bool CopyCPPFilesAfterBuild { get; set; }
        public string Player1Name { get; set; }
        public string Player2Name { get; set; }
        public string Player1Args { get; set; }
        public string Player2Args { get; set; }
        public string Player2SatisfactoryPath { get; set; }
        public string SinglePlayerArgs { get; set; }
        public double? AppHeight { get; set; }
        public double? AppWidth { get; set; }
        public bool ShowNotifications { get; set; }
    }
}
