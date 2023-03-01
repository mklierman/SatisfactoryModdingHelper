// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SatisfactoryModdingHelper.Controls
{
    public sealed partial class OutputTextControl : UserControl
    {
        private bool RunUpdateOutput = false;
        public OutputTextControl()
        {
            this.DefaultStyleKey = typeof(OutputTextControl);
            this.InitializeComponent();
        }

        private long lastFileLocation = 0;
        public async void UpdateOutput()
        {
            var path = Path.GetDirectoryName(Environment.ProcessPath) + "\\ProcessLog.txt";

            while (RunUpdateOutput)
            {
                using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    StreamReader sr = new StreamReader(fs);

                    var initialFileSize = fs.Length;
                    var newLength = initialFileSize - lastFileLocation;
                    if (newLength > 0)
                    {
                        fs.Seek(lastFileLocation, SeekOrigin.Begin);
                        while (!sr.EndOfStream)
                        {
                            var newline = sr.ReadLine();
                            if (newline != null)
                            {
                                OutputText.Add(newline);
                                //if (OutputDataGrid.Columns.Count > 0)
                                //{
                                //    OutputDataGrid.ScrollIntoView(OutputText.Last(), OutputDataGrid.Columns[0]);
                                //}
                            }
                        }
                        lastFileLocation = fs.Position;
                    }
                }
                //InputsEnabled = !_processService.ProcessRunning;
                await Task.Delay(500);

                // Highlighting regex wip
                // ^\s*(?'ProgressGroup'\[\d+\/\d+\].*$)|^.*\):\s(?'InfoType'\w+).*(?'CodeReference''.*'):\s(?'Message'.*$)
            }
        }


        private AsyncRelayCommand clearOutput;
        public ICommand ClearOutput => clearOutput ??= new AsyncRelayCommand(PerformClearOutput);
        private async Task PerformClearOutput()
        {
            var path = Path.GetDirectoryName(Environment.ProcessPath) + "\\ProcessLog.txt";
            File.WriteAllText(path, "");
            OutputText.Clear();
        }

        private ObservableCollection<string> OutputText = new();
        //public ObservableCollection<string> OutputText
        //{
        //    get => outputText;
        //    set => SetProperty(ref outputText, value);
        //}
    }
}
