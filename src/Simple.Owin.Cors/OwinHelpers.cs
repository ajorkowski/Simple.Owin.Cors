﻿namespace Simple.Owin.CorsMiddleware
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal static class OwinHelpers
    {
        public static string GetRequestHeaderValue(IDictionary<string, object> env, string key)
        {
            var headers = env.GetValueOrDefault<IDictionary<string, string[]>>(OwinKeys.RequestHeaders);
            if (headers == null) return null;
            string[] values;
            return !headers.TryGetValue(key, out values) ? null : values.FirstOrDefault();
        }

        public static T GetValueOrDefault<T>(this IDictionary<string, object> env, string key)
        {
            object obj;
            if (env.TryGetValue(key, out obj))
            {
                if (obj is T)
                {
                    return (T) obj;
                }
            }

            return default(T);
        }

        public static void SetResponseHeaderValue(IDictionary<string, object> env, string key, string value)
        {
            var headers = env.GetValueOrDefault<IDictionary<string, string[]>>(OwinKeys.ResponseHeaders);
            if (headers == null)
            {
                headers = new Dictionary<string, string[]>();
                env[OwinKeys.ResponseHeaders] = headers;
            }

            headers[key] = new[] {value};
        }

        public static void MirrorRequestMethods(IDictionary<string, object> env)
        {
            var requestHeaders = GetHeaders(env, OwinKeys.RequestHeaders);
            var responseHeaders = GetHeaders(env, OwinKeys.ResponseHeaders);
            string[] requestMethods;
            if (requestHeaders.TryGetValue("Access-Control-Request-Methods", out requestMethods))
            {
                responseHeaders["Access-Control-Allow-Methods"] = requestMethods;
            }
            else if (requestHeaders.TryGetValue("Access-Control-Request-Method", out requestMethods))
            {
                responseHeaders["Access-Control-Allow-Methods"] = requestMethods;
            }
        }

        public static void MirrorRequestHeaders(IDictionary<string, object> env)
        {
            var requestHeaders = GetHeaders(env, OwinKeys.RequestHeaders);
            var responseHeaders = GetHeaders(env, OwinKeys.ResponseHeaders);
            string[] requestMethods;
            if (requestHeaders.TryGetValue("Access-Control-Request-Headers", out requestMethods))
            {
                responseHeaders["Access-Control-Allow-Headers"] = requestMethods;
            }
            else if (requestHeaders.TryGetValue("Access-Control-Request-Header", out requestMethods))
            {
                responseHeaders["Access-Control-Allow-Headers"] = requestMethods;
            }
        }

        private static IDictionary<string, string[]> GetHeaders(IDictionary<string,object> env, string key)
        {
            var headers = env.GetValueOrDefault<IDictionary<string, string[]>>(key);
            if (headers == null)
            {
                headers = new Dictionary<string, string[]>();
                env[key] = headers;
            }
            return headers;
        }

        public static Task Stop(IDictionary<string, object> env, int statusCode)
        {
            env[OwinKeys.StatusCode] = statusCode;
            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(0);
            return tcs.Task;
        }
    }
}