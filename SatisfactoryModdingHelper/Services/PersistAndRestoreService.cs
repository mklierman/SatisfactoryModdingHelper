using System;
using System.Collections;
using System.IO;
using Microsoft.Extensions.Options;
using SatisfactoryModdingHelper.Contracts.Services;
using SatisfactoryModdingHelper.Models;

namespace SatisfactoryModdingHelper.Services
{
    public class PersistAndRestoreService : IPersistAndRestoreService
    {
        private readonly IFileService _fileService;
        private readonly AppConfig _appConfig;
        private readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public PersistAndRestoreService(IFileService fileService, IOptions<AppConfig> appConfig)
        {
            _fileService = fileService;
            _appConfig = appConfig.Value;
            RestoreData();
        }

        public SavedSettings Settings { get; set; }

        //public void PersistData()
        //{
        //    if (App.Current.Properties != null)
        //    {
        //        var folderPath = Path.Combine(_localAppData, _appConfig.ConfigurationsFolder);
        //        var fileName = _appConfig.AppPropertiesFileName;
        //        _fileService.Save(folderPath, fileName, App.Current.Properties);
        //    }
        //}

        public void PersistData()
        {
            if (Settings != null)
            {
                var folderPath = Environment.CurrentDirectory;
                var fileName = "SMH_Settings.json";
                _fileService.Save(folderPath, fileName, Settings);
            }
        }

        //public void RestoreData()
        //{
        //    var folderPath = Path.Combine(_localAppData, _appConfig.ConfigurationsFolder);
        //    var fileName = _appConfig.AppPropertiesFileName;
        //    var properties = _fileService.Read<IDictionary>(folderPath, fileName);
        //    if (properties != null)
        //    {
        //        foreach (DictionaryEntry property in properties)
        //        {
        //            App.Current.Properties.Add(property.Key, property.Value);
        //        }
        //    }
        //}

        public void RestoreData()
        {
            var folderPath = Environment.CurrentDirectory;
            var fileName = "SMH_Settings.json";
            Settings = _fileService.Read<SavedSettings>(folderPath, fileName);
            if (Settings == null)
            {
                RestoreOldData();
            }
        }

        private void RestoreOldData()
        {
            var folderPath = Path.Combine(_localAppData, _appConfig.ConfigurationsFolder);
            var fileName = _appConfig.AppPropertiesFileName;
            var properties = _fileService.Read<IDictionary>(folderPath, fileName);
            if (properties != null)
            {
                foreach (DictionaryEntry property in properties)
                {
                    App.Current.Properties.Add(property.Key, property.Value);
                }
            }
            Settings = new SavedSettings();
            Settings.CurrentPlugin = GetSavedProperty(Properties.Resources.SelectedPlugin);
            Settings.ProjectPath = GetSavedProperty(Properties.Resources.Settings_Locations_Project);
            Settings.SatisfactoryPath = GetSavedProperty(Properties.Resources.Settings_Locations_Satisfactory);
            Settings.UnrealEnginePath = GetSavedProperty(Properties.Resources.Settings_Locations_UE);
            Settings.VisualStudioPath = GetSavedProperty(Properties.Resources.Settings_Locations_VS);
            Settings.ModManagerPath = GetSavedProperty(Properties.Resources.Settings_Locations_SMM);
            //Settings.AlpakitCloseGame = GetSavedProperty(Properties.Resources.Settings_Alpakit_CloseGame_Value);
            //Settings.AlpakitCopyModToGame = GetSavedProperty(Properties.Resources.Settings_Alpakit_CopyMod_Value);
            Settings.Player1Name = GetSavedProperty(Properties.Resources.Settings_MP_Player1Name);
            Settings.Player2Name = GetSavedProperty(Properties.Resources.Settings_MP_Player2Name);
            Settings.Player1Args = GetSavedProperty(Properties.Resources.Settings_MP_Args1);
            Settings.Player2Args = GetSavedProperty(Properties.Resources.Settings_MP_Args2);
            Settings.Player2SatisfactoryPath = GetSavedProperty(Properties.Resources.Settings_MP_GameLocation);
            Settings.SinglePlayerArgs = GetSavedProperty(Properties.Resources.Settings_SP_Args);
            Settings.AppHeight = GetSavedProperty(Properties.Resources.App_Settings_Height);
            Settings.AppWidth = GetSavedProperty(Properties.Resources.App_Settings_Width);

            PersistData();
        }

        private dynamic GetSavedProperty(string propertyName)
        {
            if (App.Current.Properties.Contains(propertyName))
            {
                return App.Current.Properties[propertyName];
            }

            return null;
        }

        //public void SaveProperty(string propertyName, dynamic value)
        //{
        //    App.Current.Properties[propertyName] = value;
        //    PersistData();
        //}
    }
}
