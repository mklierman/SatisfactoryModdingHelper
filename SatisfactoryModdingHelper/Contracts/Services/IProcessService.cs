using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryModdingHelper.Contracts.Services;
public interface IProcessService
{
    public string OutputText { get; set; }
    public ObservableCollection<string> OutputList  { get; set; }
    public bool ProcessRunning
    {
        get; set;
    }
    public Task<int> RunProcess(string fileName, string arguments = "", bool redirectOutput = true);
    public void SendProcessFinishedMessage(int exitCode, string prefix);
    public void CloseRunningSatisfactoryProcesses();

    public void AddStringToOutput(string outputText);
    public void AddExceptionToOutput(string message, Exception exception);
}
