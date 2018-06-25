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
}
