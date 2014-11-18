using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace abWebapi.abRunner
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

        public Task<abRunnerResult> Run()
        {
            var tcs = new TaskCompletionSource<abRunnerResult>();

            var abRunnerResult = new abRunnerResult();
            abRunnerResult.StartDate = DateTime.UtcNow;

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
            process.ErrorDataReceived += (sender, args) => abRunnerResult.AppendOutputLine(args.Data);
            process.OutputDataReceived += (sender, args) => abRunnerResult.AppendOutputLine(args.Data);

            process.Exited += (sender, args) =>
            {
                abRunnerResult.EndDate = DateTime.UtcNow;
                tcs.SetResult(abRunnerResult);
            };

            abRunnerResult.StartDate = DateTime.UtcNow;
            process.Start();

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            return tcs.Task;
        }

        public static async Task<abRunnerResult> Run(DateTime? scheduledDate, Uri uri, int requests, int concurrency)
        {
            var abRunner = new abRunner(uri, requests, concurrency);

            // We use a datetime to schedule the running
            // of the job to make distributed synchronisation a bit easier
            // It's assumed that all ab-webapi nodes have had their system clocks
            // synced to some common source.
            if (scheduledDate.HasValue)
            {
                var timeNow = DateTime.UtcNow;
                if (scheduledDate.Value > timeNow)
                {
                    var timeUntilSchedule = scheduledDate.Value.Subtract(timeNow);
                    await Task.Delay(timeUntilSchedule);
                }
            }

            var abResult = await abRunner.Run();

            return abResult;
        }

        public static async Task<abRunnerResult> Run(Uri uri, int requests, int concurrency)
        {
            return await Run(null, uri, requests, concurrency);
        }
    }
}
