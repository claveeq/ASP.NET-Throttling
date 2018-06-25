using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace SampleThrottling.Controllers
{
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

        //public void IncrementRequestCount()
        //{
        //    _cache.AddOrUpdate(ThrottleGroup, new ThrottleInfo
        //    {
        //        ExpiresAt = DateTime.Now.AddSeconds(_timeoutInSeconds),
        //        RequestCount = 1
        //    }, (retrievedKey, throttleInfo) => 
        //    {
        //        Interlocked.Increment(ref throttleInfo.RequestCount);
        //        return throttleInfo;
        //    });
        //}

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