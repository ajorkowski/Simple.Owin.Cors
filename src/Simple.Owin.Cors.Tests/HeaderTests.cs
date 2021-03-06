﻿namespace Simple.Owin.CorsMiddleware.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class HeaderTests
    {
        private const string HostValue = "https://cors.com";
        private readonly Func<IDictionary<string,object>, Task> _next = Completed;
        
        [Fact]
        public void ItSetsAllowOriginHeader()
        {
            var env = CreateEnv();
            var func = Cors.Create(new OriginSetMatcher(HostValue)).Build();
            func(env, _next);
            Assert.Equal(HostValue, GetResponseHeader(env, HeaderKeys.AccessControlAllowOrigin));
        }
        
        [Fact]
        public void ItSetsAllowOriginHeaderToAsteriskForWildcard()
        {
            var env = CreateEnv();
            var func = Cors.Wildcard().Build();
            func(env, _next);
            Assert.Equal("*", GetResponseHeader(env, HeaderKeys.AccessControlAllowOrigin));
        }

        [Fact]
        public void ItSetsAllowCredentialsHeader()
        {
            var env = CreateEnv();
            var func = Cors.Wildcard().AllowCredentials().Build();
            func(env, _next);
            Assert.Equal("true", GetResponseHeader(env, HeaderKeys.AccessControlAllowCredentials));
        }
        
        [Fact]
        public void ItSetsAllowMethodsHeaderForSingleValue()
        {
            var env = CreateEnv();
            var func = Cors.Wildcard().AllowMethods("GET").Build();
            func(env, _next);
            Assert.Equal("GET", GetResponseHeader(env, HeaderKeys.AccessControlAllowMethods));
        }

        [Fact]
        public void ItMirrorsRequestMethodsWhenUsingWildcard()
        {
            var env = CreateEnv();
            ((IDictionary<string, string[]>) env[OwinKeys.RequestHeaders])["Access-Control-Request-Methods"] = new[] {"GET"};
            var func = Cors.Wildcard().AllowMethods("*").Build();
            func(env, _next);
            Assert.Equal("GET", GetResponseHeader(env, HeaderKeys.AccessControlAllowMethods));
        }
        
        [Fact]
        public void ItSetsAllowMethodsHeaderForManyValues()
        {
            var env = CreateEnv();
            var func = Cors.Wildcard().AllowMethods("GET", "POST", "PUT").Build();
            func(env, _next);
            Assert.Equal("GET, POST, PUT", GetResponseHeader(env, HeaderKeys.AccessControlAllowMethods));
        }

        [Fact]
        public void ItSetsAllowHeadersHeaderForSingleValue()
        {
            var env = CreateEnv();
            var func = Cors.Wildcard().AllowHeaders("X-HEADER-1").Build();
            func(env, _next);
            Assert.Equal("X-HEADER-1", GetResponseHeader(env, HeaderKeys.AccessControlAllowHeaders));
        }

        [Fact]
        public void ItSetsAllowHeadersHeaderForManyValues()
        {
            var env = CreateEnv();
            var func = Cors.Wildcard().AllowHeaders("X-HEADER-1", "X-HEADER-2", "X-HEADER-3").Build();
            func(env, _next);
            Assert.Equal("X-HEADER-1, X-HEADER-2, X-HEADER-3", GetResponseHeader(env, HeaderKeys.AccessControlAllowHeaders));
        }

        [Fact]
        public void ItMirrorsRequestHeadersWhenUsingWildcard()
        {
            var env = CreateEnv();
            ((IDictionary<string, string[]>)env[OwinKeys.RequestHeaders])["Access-Control-Request-Headers"] = new[] { "X-HEADER" };
            var func = Cors.Wildcard().AllowHeaders("*").Build();
            func(env, _next);
            Assert.Equal("X-HEADER", GetResponseHeader(env, HeaderKeys.AccessControlAllowHeaders));
        }
        

        [Fact]
        public void ItSetsExposeHeadersHeaderForSingleValue()
        {
            var env = CreateEnv();
            var func = Cors.Wildcard().ExposeHeaders("X-HEADER-1").Build();
            func(env, _next);
            Assert.Equal("X-HEADER-1", GetResponseHeader(env, HeaderKeys.AccessControlExposeHeaders));
        }

        [Fact]
        public void ItSetsExposeHeadersHeaderForManyValues()
        {
            var env = CreateEnv();
            var func = Cors.Wildcard().ExposeHeaders("X-HEADER-1", "X-HEADER-2", "X-HEADER-3").Build();
            func(env, _next);
            Assert.Equal("X-HEADER-1, X-HEADER-2, X-HEADER-3", GetResponseHeader(env, HeaderKeys.AccessControlExposeHeaders));
        }

        [Fact]
        public void ItSetsMaxAgeHeaderFromNumber()
        {
            var env = CreateEnv();
            var func = Cors.Wildcard().MaxAge(3600).Build();
            func(env, _next);
            Assert.Equal("3600", GetResponseHeader(env, HeaderKeys.AccessControlMaxAge));
        }

        [Fact]
        public void ItSetsMaxAgeHeaderFromTimeSpan()
        {
            var env = CreateEnv();
            var timeSpan = TimeSpan.FromDays(1);
            var func = Cors.Wildcard().MaxAge(timeSpan).Build();
            func(env, _next);
            Assert.Equal(timeSpan.TotalSeconds.ToString(CultureInfo.InvariantCulture), GetResponseHeader(env, HeaderKeys.AccessControlMaxAge));
        }

        private static IDictionary<string, object> CreateEnv()
        {
            return new Dictionary<string, object>
            {
                {
                    OwinKeys.RequestHeaders, new Dictionary<string, string[]>
                    {
                        {"Host", new[] {HostValue}}
                    }
                },
                {
                    OwinKeys.ResponseHeaders, new Dictionary<string, string[]>()
                }
            };
        }

        private static string GetResponseHeader(IDictionary<string, object> env, string key)
        {
            var headers = (IDictionary<string, string[]>) env[OwinKeys.ResponseHeaders];
            string[] value;
            if (headers.TryGetValue(key, out value))
            {
                return value.FirstOrDefault();
            }
            return null;
        }

        private static Task Completed(IDictionary<string,object> _)
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(0);
            return tcs.Task;
        }
    }
}