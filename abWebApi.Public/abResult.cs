using System;
using System.Collections.Generic;

namespace abWebapi.Public
{
    public class abResult
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string ServerSoftware { get; set; }
        public string ServerHostname { get; set; }
        public int ServerPort { get; set; }

        public string DocumentPath { get; set; }
        public ulong DocumentLength { get; set; }

        public int ConcurrencyLevel { get; set; }
        public double TimeTakenForTestsMs { get; set; }
        public int CompleteRequests { get; set; }
        public int FailedRequests { get; set; }
        public ulong TotalTransferred { get; set; }
        public ulong HtmlTransferred { get; set; }
        public double AverageRequestsPerSecond { get; set; }
        public double AverageTimePerRequestMs { get; set; }
        public double AverageTimePerRequestAcrossAllRequestsMs { get; set; }
        public double TransferRateKBytesPerSec { get; set; }

        public Dictionary<int, double> ResponseTimePercentageSplits { get; set; }

        public string[] RawOutput { get; set; }
    }
}
