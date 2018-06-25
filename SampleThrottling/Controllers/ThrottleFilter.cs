using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace SampleThrottling.Controllers
{
    public class ThrottleFilter : ActionFilterAttribute
    {
        private Throttler _throttler;
        private string _throttleGroup;

        public ThrottleFilter(
            int RequestLimit = 5,
            int TimeoutInSeconds = 10,
            [CallerMemberName] string ThrottleGroup = null)
        {
            _throttleGroup = ThrottleGroup;
            _throttler = new Throttler(ThrottleGroup, RequestLimit, TimeoutInSeconds);
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            setIdentityAsThrottleGroup();

            if (_throttler.RequestShouldBeThrottled)
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    (HttpStatusCode)429, "Too many requests");

                addThrottleHeaders(actionContext.Response);
            }

            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            setIdentityAsThrottleGroup();
            if (actionExecutedContext.Exception == null) _throttler.IncrementRequestCount();
            addThrottleHeaders(actionExecutedContext.Response);
            base.OnActionExecuted(actionExecutedContext);
        }

        private void setIdentityAsThrottleGroup()
        {
            if (_throttleGroup == "identity")
                _throttler.ThrottleGroup = HttpContext.Current.User.Identity.Name;

            if (_throttleGroup == "ipaddress")
                _throttler.ThrottleGroup = HttpContext.Current.Request.UserHostAddress;
        }

        private void addThrottleHeaders(HttpResponseMessage response)
        {
            if (response == null) return;

            foreach (var header in _throttler.GetRateLimitHeaders())
                response.Headers.Add(header.Key, header.Value);
        }
    }
}