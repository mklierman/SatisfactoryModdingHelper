using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.UI.Controls;
using SatisfactoryModdingHelper.Contracts.Services;
using Windows.System;
using static System.Net.Mime.MediaTypeNames;

namespace SatisfactoryModdingHelper.Services;
public class ProcessService : ObservableRecipient, IProcessService
{
    public ProcessService()
    {
        outputText = "";
        processRunning = false;
        dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }
    public DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private string outputText;

    public string OutputText
    {
        get => outputText;
        set => SetProperty(ref outputText, value);
    }

    private ObservableCollection<string> outputList = new();
    public ObservableCollection<string> OutputList
    {
        get => outputList;
        set => SetProperty(ref outputList, value);
    }

    private bool processRunning;
    public bool ProcessRunning
    {
        get => processRunning;
        set => SetProperty(ref processRunning, value);
    }

    public static DispatcherQueue? ProcessServiceThread = null;

    public async Task<int> RunProcess(string fileName, string arguments = "", bool redirectOutput = true)
    {
        try
        {
            ProcessServiceThread = DispatcherQueue.GetForCurrentThread();
            var taskCompletionSource = new TaskCompletionSource<int>();
            Process process = new()
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardError = true,
                    RedirectStandardOutput = redirectOutput,
                    WorkingDirectory = "C:\\"
                }
            };
            //process.OutputDataReceived += (s, e) => {
            //    lock (OutputList)
            //    {
            //       // OutputList.Add(e.Data ?? "");
            //        App.MainWindow.DispatcherQueue.TryEnqueue(() => { OutputList.Add(e.Data ?? ""); });
            //    }
            //};
            //process.ErrorDataReceived += (s, e) => {
            //    lock (OutputList)
            //    {
            //       // OutputList.Add("! > " + e.Data ?? "");
            //        App.MainWindow.DispatcherQueue.TryEnqueue(() => { OutputList.Add("! > " + e.Data ?? ""); });
            //    }
            //};
            process.OutputDataReceived += Cmd_DataReceived;
            process.ErrorDataReceived += Cmd_DataReceived;
            process.Exited += (sender, args) =>
            {
                taskCompletionSource.SetResult(process.ExitCode);
                process.Dispose();
            };

            ProcessRunning = true;
            process.Start();
            if (redirectOutput)
            {
                process.BeginOutputReadLine();
            }
            await process.WaitForExitAsync();
            ProcessRunning = false;
            return taskCompletionSource.Task.Result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("");
        }
        return -1;
    }

    private void Cmd_DataReceived(object sender, DataReceivedEventArgs e)
    {
        var path = Path.GetDirectoryName(Environment.ProcessPath) + "\\ProcessLog.txt";
        File.AppendAllText(path, (e.Data ?? "") + Environment.NewLine);
    }

    public void SendProcessFinishedMessage(int exitCode, string prefix)
    {
        var resultOutputText = exitCode switch
        {
            0 => $"{prefix} Successful",
            2 => $"{prefix} Failed: Unable to find a needed file. Double check your directory paths",
            3 => $"{prefix} Failed: Unable to find a needed path. Double check your directory paths",
            5 => $"{prefix} Failed: Access Denied to something",
            _ => $"{prefix} Failed",
        };
        AddStringToOutput(resultOutputText);
    }

    public void CloseRunningSatisfactoryProcesses()
    {
        var processlist = Process.GetProcessesByName("Satisfactory.exe");
        if (processlist.Length > 0)
        {
            AddStringToOutput("Stopping existing Satisfactory processess...");
            foreach (var process in processlist)
            {
                var procName = process.ProcessName;
                process.Kill();
                AddStringToOutput($"{procName} has been stopped");
            }
        }
    }

    public static IntPtr GetAppHWND()
    {
        using (var proc = Process.GetProcessById(Environment.ProcessId))
        {
            return proc.MainWindowHandle;
        }
    }

    public void AddStringToOutput(string outputText)
    {
        var path = Path.GetDirectoryName(Environment.ProcessPath) + "\\ProcessLog.txt";
        File.AppendAllText(path, (outputText ?? "") + Environment.NewLine);
    }
}
