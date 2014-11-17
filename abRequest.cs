using System;

namespace abWebapi
{
    /*
        {
          "ScheduledDate": "2015-01-01T00:00:00Z",
          "Uri": "http://192.168.1.198:9100/values",
          "Requests": 1000,
          "Concurrency": 2
        }
    */
    public class abRequest
    {
        public DateTimeOffset? SheduledDate { get; set; }
        public Uri Uri { get; set; }
        public int Requests { get; set; }
        public int Concurrency { get; set; }
    }
}
