using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace abWebapi
{
    public class abController : ApiController
    {
        // POST: /ab
        [HttpPost]
        public async Task<string> Get([FromBody]abRequest abRequest)
        {
            var abRunner = new abRunner(abRequest.Uri, abRequest.Requests, abRequest.Concurrency);

            // We use a datetime to schedule the running
            // of the job to make distributed synchronisation a bit easier
            // It's assumed that all ab-webapi nodes have had their system clocks
            // synced to some common source.
            if (abRequest.SheduledDate.HasValue)
            {
                var timeNow = DateTime.UtcNow;
                if (abRequest.SheduledDate.Value.UtcDateTime > timeNow)
                {
                    var timeUntilSchedule = abRequest.SheduledDate.Value.UtcDateTime.Subtract(timeNow);
                    await Task.Delay(timeUntilSchedule);
                }
            }

            var abResult = await abRunner.Run();

            return abResult;
        }
    } 
}
