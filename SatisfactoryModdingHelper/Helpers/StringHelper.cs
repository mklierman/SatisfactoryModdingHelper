using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryModdingHelper.Helpers
{
    //Helper class for command line stuff
    public static class StringHelper
    {
        public const string RunningAlpakit = "Running Alpakit...";
        public const string AlpakitComplete = "Alpakit Complete";

        public static string BuildAlpakitString(bool shouldCopyMod,string satisfactoryFolderPath, string enginePath, string uprojectFilePath, string modName)
        {
            var alpakitArgs = shouldCopyMod ? $" -CopyToGameDir -GameDir=`{satisfactoryFolderPath}`" : "";
            return "";
        }
    }
}
