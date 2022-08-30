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
        public string ProjectPath { get; set; }
        public string SatisfactoryPath { get; set; }
        public string UnrealEnginePath { get; set; }
        public string VisualStudioPath { get; set; }
        public string ModManagerPath { get; set; }
        public bool AlpakitCloseGame { get; set; }
        public bool AlpakitCopyModToGame { get; set; }
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
