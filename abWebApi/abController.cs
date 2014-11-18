using System.Threading.Tasks;
using System.Web.Http;
using abWebapi.Public;

namespace abWebapi
{
    public class abController : ApiController
    {
        // POST: /ab
        [HttpPost]
        public async Task<abResult> Get([FromBody]abRequest abRequest)
        {
            var abRunnerResult = await abRunner.abRunner.Run(abRequest.SheduledDate, abRequest.Uri, abRequest.Requests, abRequest.Concurrency);
            var abResult = new abResult();

            abResult.StartDate = abRunnerResult.StartDate;
            abResult.EndDate = abRunnerResult.EndDate;

            abResult.ServerSoftware = abRunnerResult.ServerSoftware;
            abResult.ServerHostname = abRunnerResult.ServerHostname;
            abResult.ServerPort = abRunnerResult.ServerPort;

            abResult.DocumentPath = abRunnerResult.DocumentPath;
            abResult.DocumentLength = abRunnerResult.DocumentLength;

            abResult.ConcurrencyLevel = abRunnerResult.ConcurrencyLevel;
            abResult.TimeTakenForTestsMs = abRunnerResult.TimeTakenForTestsMs;
            abResult.CompleteRequests = abRunnerResult.CompleteRequests;
            abResult.FailedRequests = abRunnerResult.FailedRequests;
            abResult.TotalTransferred = abRunnerResult.TotalTransferred;
            abResult.HtmlTransferred = abRunnerResult.HtmlTransferred;
            abResult.AverageRequestsPerSecond = abRunnerResult.AverageRequestsPerSecond;
            abResult.AverageTimePerRequestMs = abRunnerResult.AverageTimePerRequestMs;
            abResult.AverageTimePerRequestAcrossAllRequestsMs = abRunnerResult.AverageTimePerRequestAcrossAllRequestsMs;
            abResult.TransferRateKBytesPerSec = abRunnerResult.TransferRateKBytesPerSec;

            abResult.ResponseTimePercentageSplits = abRunnerResult.ResponseTimePercentageSplits;

            abResult.RawOutput = abRunnerResult.RawOutput;

            return abResult;
        }
    } 
}
