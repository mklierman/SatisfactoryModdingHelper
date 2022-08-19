using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SatisfactoryModdingHelper.Contracts.Services;

namespace SatisfactoryModdingHelper.Services;
public class ProcessService : ObservableRecipient, IProcessService
{
    public ProcessService()
    {
        outputText = "";
    }

    private string outputText;

    public string OutputText
    {
        get => outputText;
        set => SetProperty(ref outputText, value);
    }

    public async Task<int> RunProcess(string fileName, string arguments = "", bool redirectOutput = true)
    {
        try
        {

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
                    RedirectStandardOutput = redirectOutput
                }
            };

            process.OutputDataReceived += Cmd_DataReceived;

            process.Exited += (sender, args) =>
            {
                taskCompletionSource.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();
            if (redirectOutput)
            {
                process.BeginOutputReadLine();
            }
            await process.WaitForExitAsync();
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
        OutputText += e.Data + Environment.NewLine;
    }

    public void SendProcessFinishedMessage(int exitCode, string prefix)
    {
        OutputText += exitCode switch
        {
            0 => $"{prefix} Successful",
            2 => $"{prefix} Failed: Unable to find a needed file. Double check your directory paths",
            3 => $"{prefix} Failed: Unable to find a needed path. Double check your directory paths",
            5 => $"{prefix} Failed: Access Denied to something",
            _ => $"{prefix} Failed",
        };
    }

    public void CloseRunningSatisfactoryProcesses()
    {
        var processlist = Process.GetProcessesByName("Satisfactory.exe");
        if (processlist.Length > 0)
        {
            OutputText = "Stopping existing Satisfactory processess..." + Environment.NewLine;
            foreach (var process in processlist)
            {
                process.Kill();
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
}
