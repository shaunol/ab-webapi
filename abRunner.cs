using System;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace abWebapi
{
    internal class abRunner
    {
        public Uri Uri { get; private set; }
        public int Requests { get; private set; }
        public int Concurrency { get; private set; }

        public abRunner(Uri uri, int requests, int concurrency)
        {
            Uri = uri;
            Requests = requests;
            Concurrency = concurrency;
        }

        public Task<string> Run()
        {
            var tcs = new TaskCompletionSource<string>();

            var result = new StringBuilder();

            var startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = ConfigurationManager.AppSettings["abPath"];
            startInfo.Arguments = String.Format("-n {0} -c {1} {2}", Requests, Concurrency, Uri);

            var process = new Process();
            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;
            process.ErrorDataReceived += (sender, args) => result.AppendLine(args.Data);
            process.OutputDataReceived += (sender, args) => result.AppendLine(args.Data);
            process.Exited += (sender, args) => tcs.SetResult(result.ToString());            
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            return tcs.Task;
        }
    }
}
