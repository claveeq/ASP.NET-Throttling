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
        //NEW GET
        [HttpGet]
        [Route("~/api/helloworld")]
        public HttpResponseMessage HelloWorld()
        {
            var throttler = new Throttler("helloworld");

            if (throttler.RequestShouldBeThrottled())
                return Request.CreateResponse(
                    (HttpStatusCode)429, "Too many requests");

            return Request.CreateResponse(HttpStatusCode.OK, "Hello World");
        }

        [HttpGet]
        [Route("~/api/updatesomething")]
        public HttpResponseMessage UpdateSomething()
        {
            var throttler = new Throttler("updatesomething");

            if (throttler.RequestShouldBeThrottled())
                return Request.CreateResponse(
                    (HttpStatusCode)429, "Too many requests");

            // update something here

            return Request.CreateResponse(HttpStatusCode.OK, "Data updated");
        }

        // GET api/values
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
        private int _requestLimit;
        private int _timeoutInSeconds;
        private string _key;
        private static ConcurrentDictionary<string, ThrottleInfo> _cache =
            new ConcurrentDictionary<string, ThrottleInfo>();

        public Throttler(string key, int requestLimit = 5, int timeoutInSeconds = 10)
        {
            _requestLimit = requestLimit;
            _timeoutInSeconds = timeoutInSeconds;
            _key = key;
        }

        public bool RequestShouldBeThrottled()
        {
            ThrottleInfo throttleInfo = _cache.ContainsKey(_key) ? _cache[_key] : null;

            if (throttleInfo == null || throttleInfo.ExpiresAt <= DateTime.Now)
            {
                throttleInfo = new ThrottleInfo
                {
                    ExpiresAt = DateTime.Now.AddSeconds(_timeoutInSeconds),
                    RequestCount = 0
                };
            };

            throttleInfo.RequestCount++;

            _cache[_key] = throttleInfo;

            return (throttleInfo.RequestCount > _requestLimit);
        }

        private class ThrottleInfo
        {
            public DateTime ExpiresAt { get; set; }
            public int RequestCount { get; set; }
        }
    }
}
