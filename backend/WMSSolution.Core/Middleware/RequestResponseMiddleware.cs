using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using WMSSolution.Core.Utility;

namespace WMSSolution.Core.Middleware
{
    /// <summary>
    /// Request response middleware
    /// </summary>
    public class RequestResponseMiddleware
    {
        #region parameter
        /// <summary>
        /// Delegate
        /// </summary>
        private readonly RequestDelegate _next;
        /// <summary>
        /// log manager
        /// </summary>
        private readonly ILogger<RequestResponseMiddleware> _logger;

        /// <summary>
        /// constant for log body size
        /// </summary>
        private const int MaxBodyLogSize = 4096;// 4KB

        /// <summary>
        /// Define sensitive endpoints that should not log request and response body
        /// </summary>
        private static readonly HashSet<string> _sensitiveEndpoints = new(StringComparer.OrdinalIgnoreCase)
        {
        };

        /// <summary>
        /// Define sensitive fields that should be masked in logs, even for non-sensitive endpoints
        /// </summary>
        private static readonly HashSet<string> _sensitiveFields = new(StringComparer.OrdinalIgnoreCase)
        {

        };

        #endregion



        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">Delegate</param>
        /// <param name="logger">log manager</param>
        public RequestResponseMiddleware(RequestDelegate next
            , ILogger<RequestResponseMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        #endregion

        /// <summary>
        /// invoke method for request response middleware
        /// </summary>
        /// <param name="context">httpcontext</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            if (!GlobalConsts.IsRequestResponseMiddleware)
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? string.Empty;
            var queryString = context.Request.QueryString;
            var clientIp = GetClientIp(context);

            // read request body 
            //  var requestBody = await ReadAndRedactBodyAsync(context.Request, path);
            var requestBody = string.Empty;

            await _next(context);

            stopwatch.Stop();

            //Structured logging 
            _logger.LogInformation(
               "HTTP {Method} {Path}{QueryString} | IP: {ClientIp} | Status: {StatusCode} | Time: {ElapsedMs}ms | Body: {RequestBody}",
               method, path, queryString, clientIp, context.Response.StatusCode,
               stopwatch.ElapsedMilliseconds, requestBody);
        }

        private static string GetClientIp(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                var ip = forwardedFor.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                if (!string.IsNullOrWhiteSpace(ip))
                {
                    return ip;
                }
            }

            if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
            {
                var ip = realIp.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(ip))
                {
                    return ip;
                }
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        /// <summary>
        /// Read request body with rules 
        /// </summary>
        private static async Task<string> ReadAndRedactBodyAsync(HttpRequest request, string path)
        {
            if (_sensitiveEndpoints.Contains(path))
            {
                return "[REDACTED - Sensitive endpoint]";
            }

            if (request.ContentLength is null or 0)
            {
                return string.Empty;
            }

            request.EnableBuffering();

            bool isTruncated = request.ContentLength > MaxBodyLogSize;
            var bufferSize = (int)Math.Min(request.ContentLength.Value, MaxBodyLogSize);
            var buffer = new byte[bufferSize];
            var bytesRead = await request.Body.ReadAsync(buffer.AsMemory(0, bufferSize));
            request.Body.Position = 0;

            var body = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            if (!isTruncated)
            {
                return RedactSensitiveFields(body);
            }
            if (ContainsSensitiveField(body))
            {
                return $"[REDACTED & TRUNCATED - Total: {request.ContentLength} bytes]";
            }

            return body + $"[TRUNCATED - Total: {request.ContentLength} bytes]";
        }

        private static bool ContainsSensitiveField(string body)
        {
            foreach (var field in _sensitiveFields)
            {
                if (body.Contains($"\"{field}\"", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Mask sensitive field
        /// </summary>
        private static string RedactSensitiveFields(string body)
        {
            if (_sensitiveFields.Count == 0
                || string.IsNullOrWhiteSpace(body)
                || body[0] != '{')
            {
                return body;
            }

            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return body;
                }

                var dict = new Dictionary<string, object?>();
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    dict[prop.Name] = _sensitiveFields.Contains(prop.Name)
                        ? "REDACTED property"
                        : prop.Value.ToString();
                }

                return JsonSerializer.Serialize(dict);
            }
            catch
            {
                return body;
            }
        }

    }
}
