using System;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace SatisfactoryModdingHelper.ViewModels
{
    public class UPluginViewModel : ObservableObject
    {
        public UPluginViewModel()
        {
        }

        private string fileVersion;

        public string FileVersion { get => fileVersion; set => SetProperty(ref fileVersion, value); }
    }
}
