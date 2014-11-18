using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace abWebapi.abRunner
{
    public class abRunnerResult
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

        public readonly Dictionary<int, double> ResponseTimePercentageSplits = new Dictionary<int, double>();

        private readonly List<string> _RawOutput = new List<string>(); 
        public string[] RawOutput { get { return _RawOutput.ToArray(); } }

        private bool lastAppendLineWasEmpty;
        private bool appendLineIsInResultsMode;
        private bool appendLineIsInConnectionTimes;
        private bool appendLineIsInPercentageSplits;
        private bool parseComplete;

        public void AppendOutputLine(string outputLine)
        {
            _RawOutput.Add(outputLine);

            if (String.IsNullOrEmpty(outputLine))
            {
                if (lastAppendLineWasEmpty)
                {
                    if (!appendLineIsInResultsMode)
                    {
                        appendLineIsInResultsMode = true;
                    }
                    else
                    {
                        parseComplete = true;
                    }
                }
                lastAppendLineWasEmpty = true;
                return;
            }

            lastAppendLineWasEmpty = false;

            if (appendLineIsInResultsMode)
            {
                if (outputLine == "Connection Times (ms)")
                {
                    appendLineIsInConnectionTimes = true;
                    return;
                }

                if (outputLine == "Percentage of the requests served within a certain time (ms)")
                {
                    appendLineIsInPercentageSplits = true;
                    return;
                }

                if (appendLineIsInConnectionTimes)
                {
                    if (appendLineIsInPercentageSplits)
                    {
                        ProcessResponseTimePercentageSplits(outputLine);
                        
                    }
                    else
                    {
                        ProcessConnectionTimesTable(outputLine);
                    }
                }
                else
                {
                    ProcessResultsKeyValues(outputLine);
                }
            }
        }

        private readonly Regex _ResultsKeyValuesRegex = new Regex(@"^([\S ]+):\s+([\S \d]+)$");
        private readonly Regex _ResponseTimesTableRegex = new Regex(@"(\d+)%\W+(\d+)");

        private void ProcessResultsKeyValues(string outputLine)
        {
            var matches = _ResultsKeyValuesRegex.Match(outputLine);
            if (matches.Groups.Count == 3)
            {
                var key = matches.Groups[1].Value;
                var value = matches.Groups[2].Value;

                switch (key)
                {
                    case "Server Software":
                        ServerSoftware = value;
                        break;

                    case "Server Hostname":
                        ServerHostname = value;
                        break;

                    case "Server Port":
                        int port;
                        Int32.TryParse(value, out port);
                        ServerPort = port;
                        break;

                    case "Document Path":
                        DocumentPath = value;
                        break;

                    case "Document Length":
                        DocumentLength = ProcessSizeInBytesString(value);
                        break;

                    case "Concurrency Level":
                        int concurrencyLevel;
                        Int32.TryParse(value, out concurrencyLevel);
                        ConcurrencyLevel = concurrencyLevel;
                        break;

                    case "Time taken for tests":
                        // TODO: Process as timestamp: "0.115 seconds" is it always seconds?
                        if (value.EndsWith("seconds"))
                        {
                            double secondsTaken;
                            double.TryParse(value.Substring(0, value.IndexOf(" ", StringComparison.InvariantCultureIgnoreCase)), out secondsTaken);
                            TimeTakenForTestsMs = secondsTaken * 1000;
                        }
                        else
                        {
                            throw new NotImplementedException("Time taken for tests format not handled: " + value);
                        }
                        break;

                    case "Complete Requests":
                        int completeRequests;
                        Int32.TryParse(value, out completeRequests);
                        CompleteRequests = completeRequests;
                        break;

                    case "Failed Requests":
                        int failedRequests;
                        Int32.TryParse(value, out failedRequests);
                        FailedRequests = failedRequests;
                        break;

                    case "Total transferred":
                        TotalTransferred = ProcessSizeInBytesString(value);
                        break;

                    case "HTML transferred":
                        HtmlTransferred = ProcessSizeInBytesString(value);
                        break;

                    case "Requests per second":
                        double requestsPerSecond;
                        double.TryParse(value.Substring(0, value.IndexOf(" ", StringComparison.InvariantCultureIgnoreCase)), out requestsPerSecond);
                        AverageRequestsPerSecond = requestsPerSecond;
                        break;

                    case "Time per request":
                        // DOUBLE LINE HANDLER
                        if (AverageTimePerRequestMs <= 0)
                        {
                            // TODO Handle "2.300 [ms] (mean)" is it always ms?
                            if (value.EndsWith("[ms] (mean)"))
                            {
                                double msTaken;
                                double.TryParse(value.Substring(0, value.IndexOf(" ", StringComparison.InvariantCultureIgnoreCase)), out msTaken);
                                AverageTimePerRequestMs = msTaken;
                            }
                            else
                            {
                                throw new NotImplementedException("Time per request format not handled: " + value);
                            }
                        }
                        else
                        {
                            // TODO Handle "1.150 [ms] (mean, across all concurrent requests)" is it always ms?
                            if (value.EndsWith("[ms] (mean, across all concurrent requests)"))
                            {
                                double msTaken;
                                double.TryParse(value.Substring(0, value.IndexOf(" ", StringComparison.InvariantCultureIgnoreCase)), out msTaken);
                                AverageTimePerRequestAcrossAllRequestsMs = msTaken;
                            }
                            else
                            {
                                throw new NotImplementedException("Time per request (AcrossAllRequests) format not handled: " + value);
                            }
                        }
                        break;

                    case "Transfer rate":
                        // TODO: Process: "178.32 [Kbytes/sec] received" is it always Kbytes/sec?
                        if (value.EndsWith("[Kbytes/sec] received"))
                        {
                            double kbytesPerSec;
                            double.TryParse(value.Substring(0, value.IndexOf(" ", StringComparison.InvariantCultureIgnoreCase)), out kbytesPerSec);
                            TransferRateKBytesPerSec = kbytesPerSec;
                        }
                        else
                        {
                            throw new NotImplementedException("Transfer rate format not handled: " + value);
                        }
                        break;
                }
            }
        }

        private void ProcessResponseTimePercentageSplits(string outputLine)
        {
            var matches = _ResponseTimesTableRegex.Match(outputLine);
            if (matches.Groups.Count == 3)
            {
                var percentString = matches.Groups[1].Value;
                var timeTakenString = matches.Groups[2].Value;

                int percent;
                Int32.TryParse(percentString, out percent);

                double timeTakenMs;
                double.TryParse(timeTakenString, out timeTakenMs);

                ResponseTimePercentageSplits.Add(percent, timeTakenMs);
            }
        }

        private void ProcessConnectionTimesTable(string outputLine)
        {

        }

        private ulong ProcessSizeInBytesString(string input)
        {
            ulong bytes;

            // TODO: Is it always bytes?
            if (input.EndsWith("bytes"))
            {
                ulong.TryParse(input.Substring(0, input.IndexOf(" ", StringComparison.InvariantCultureIgnoreCase)), out bytes);
            }
            else
            {
                throw new NotImplementedException("SizeInBytesStringProcess format not handled: " + input);
            }

            return bytes;
        }
    }
}
