using System;
using System.Diagnostics;
using System.Threading.Tasks;

static class Process 
{
    public async static Task<string> RunAsync(string file, string args)
    {
        System.Diagnostics.Process process = null;
        string responseString = "";
        DataReceivedEventHandler response = (_, n) => responseString += n.Data + "\n";
        DataReceivedEventHandler onError = (_, n) => Console.WriteLine(n.Data);
        EventHandler exited = null;
        try 
        {
            process = new()
            {
                StartInfo = new()
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = file,
                    Arguments = args,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            process.OutputDataReceived += response;
            process.ErrorDataReceived += onError;

            var tcs = new TaskCompletionSource<int>();
            exited = (_, __) => tcs.SetResult(0);

            process.Exited += exited;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await tcs.Task;
            return responseString;
        }
        finally
        {
            process.OutputDataReceived -= response;
            process.ErrorDataReceived -= onError;
            if (exited != null)
                process.Exited -= exited;
        }
    }
}