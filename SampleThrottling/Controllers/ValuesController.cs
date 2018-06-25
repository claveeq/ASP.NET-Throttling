using System;
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

        private class ThrottleInfo
        {
            public DateTime ExpiresAt { get; set; }
            public int RequestCount { get; set; }
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

        public Throttler(string key, int requestLimit = 5, int timeoutInSeconds = 10)
        {
            _requestLimit = requestLimit;
            _timeoutInSeconds = timeoutInSeconds;
            _key = key;
        }

        public bool RequestShouldBeThrottled()
        {
            ThrottleInfo throttleInfo = (ThrottleInfo)HttpRuntime.Cache[_key];

            if (throttleInfo == null) throttleInfo = new ThrottleInfo
            {
                ExpiresAt = DateTime.Now.AddSeconds(_timeoutInSeconds),
                RequestCount = 0
            };

            throttleInfo.RequestCount++;

            HttpRuntime.Cache.Add(_key,
          throttleInfo,
          null,
          throttleInfo.ExpiresAt,
          Cache.NoSlidingExpiration,
          CacheItemPriority.Normal,
          null);

            return (throttleInfo.RequestCount > _requestLimit);
        }

        private class ThrottleInfo
        {
            public DateTime ExpiresAt { get; set; }
            public int RequestCount { get; set; }
        }
    }
}
