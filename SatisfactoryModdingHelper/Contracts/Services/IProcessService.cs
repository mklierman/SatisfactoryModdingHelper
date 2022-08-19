using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryModdingHelper.Contracts.Services;
public interface IProcessService
{
    public string OutputText { get; set; }
    public Task<int> RunProcess(string fileName, string arguments = "", bool redirectOutput = true);
    public void SendProcessFinishedMessage(int exitCode, string prefix);
    public void CloseRunningSatisfactoryProcesses();
}
