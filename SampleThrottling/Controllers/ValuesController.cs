using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Caching;
using System.Web.Http;

namespace SampleThrottling.Controllers
{
    //[Authorize]
    public class ValuesController : ApiController
    {
        //Basic usage for throttling requests:
        [ThrottleFilter()]
        [HttpGet]
        [Route("~/api/helloworld")]
        public HttpResponseMessage HelloWorld()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Hello World");
        }
        //Allow more requests through, say 50 every 5 seconds:
        [ThrottleFilter(RequestLimit: 50, TimeoutInSeconds: 5)]
        [HttpGet]
        [Route("~/api/allow-more")]
        public HttpResponseMessage HelloWorld2()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Hello World2");
        }

        //Throttling a group of requests together:
        [ThrottleFilter(ThrottleGroup: "updates")]
        [HttpPost]
        [Route("~/api/name")]
        public HttpResponseMessage UpdateName()
        {
            // update name here
            return Request.CreateResponse(HttpStatusCode.OK, "Name updated ok");
        }

        [ThrottleFilter(ThrottleGroup: "updates")]
        [HttpPost]
        [Route("~/api/address")]
        public HttpResponseMessage UpdateAddress()
        {
            // update address here
            return Request.CreateResponse(HttpStatusCode.OK, "Address updated ok");
        }

        //Throttling based on IP address:
        [ThrottleFilter(ThrottleGroup: "ipaddress")]
        [HttpGet]
        [Route("~/api/nameByIp")]
        public HttpResponseMessage GetNameByIp(int id)
        {
            return Request.CreateResponse(HttpStatusCode.OK, "John Smith");
        }

        //Throttling based on Identity:
        [ThrottleFilter(ThrottleGroup: "identity")]
        [HttpGet]
        [Route("~/api/name")]
        public HttpResponseMessage GetName(int id)
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Jane Doe");
        }

        // GET api/values
        [ThrottleFilter()]
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new string[] { "value1", "value2" });
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }

    public class Throttler
    {
        public int RequestLimit { get; private set; }
        public int RequestsRemaining { get; private set; }
        public DateTime WindowResetDate { get; private set; }
        private static ConcurrentDictionary<string, ThrottleInfo> _cache =
            new ConcurrentDictionary<string, ThrottleInfo>();

        public string ThrottleGroup { get; set; }
        private int _timeoutInSeconds;

        public Throttler(string key, int requestLimit = 5, int timeoutInSeconds = 10)
        {
            RequestLimit = requestLimit;
            _timeoutInSeconds = timeoutInSeconds;
            ThrottleGroup = key;
        }

        private ThrottleInfo getThrottleInfoFromCache()
        {
            ThrottleInfo throttleInfo =
                _cache.ContainsKey(ThrottleGroup) ? _cache[ThrottleGroup] : null;

            if (throttleInfo == null || throttleInfo.ExpiresAt <= DateTime.Now)
            {
                throttleInfo = new ThrottleInfo
                {
                    ExpiresAt = DateTime.Now.AddSeconds(_timeoutInSeconds),
                    RequestCount = 0
                };
            };

            return throttleInfo;
        }

        public bool RequestShouldBeThrottled
        {
            get
            {
                ThrottleInfo throttleInfo = getThrottleInfoFromCache();
                WindowResetDate = throttleInfo.ExpiresAt;
                RequestsRemaining = Math.Max(RequestLimit - throttleInfo.RequestCount, 0);
                return (throttleInfo.RequestCount > RequestLimit);
            }
        }

        public void IncrementRequestCount()
        {
            ThrottleInfo throttleInfo = getThrottleInfoFromCache();
            throttleInfo.RequestCount++;
            _cache[ThrottleGroup] = throttleInfo;
        }

        private class ThrottleInfo
        {
            public DateTime ExpiresAt { get; set; }
            public int RequestCount { get; set; }
        }

        public Dictionary<string, string> GetRateLimitHeaders()
        {
            ThrottleInfo throttleInfo = getThrottleInfoFromCache();

            int requestsRemaining = Math.Max(RequestLimit - throttleInfo.RequestCount, 0);

            var headers = new Dictionary<string, string>();
            headers.Add("X-RateLimit-Limit", RequestLimit.ToString());
            headers.Add("X-RateLimit-Remaining", RequestsRemaining.ToString());
            headers.Add("X-RateLimit-Reset", toUnixTime(throttleInfo.ExpiresAt).ToString());
            return headers;
        }

        private long toUnixTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date.ToUniversalTime() - epoch).TotalSeconds);
        }

    }
}
