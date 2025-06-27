using System.Runtime.Caching;
using System.Web.Mvc;
using System;
using System.Net;
using System.Collections.Generic; // For dictionary
using System.Linq; // For string.Join

public class ThrottlingFilterAttribute : ActionFilterAttribute
{
    public int MaxRequests { get; set; } = 100;
    public int TimeWindowMinutes { get; set; } = 1;

    // New property: specify parameters to include in the throttling key
    // Example: "username", "email"
    public string KeyParameters { get; set; } = "";

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var request = filterContext.HttpContext.Request;
        var ipAddress = request.UserHostAddress;

        if (string.IsNullOrEmpty(ipAddress))
        {
            base.OnActionExecuting(filterContext);
            return;
        }

        var cache = MemoryCache.Default;

        // Base cache key: IP + Action Name
        string cacheKeyBase = $"Throttle_{ipAddress}_{filterContext.ActionDescriptor.ActionName}";
        string finalCacheKey = cacheKeyBase;

        // Append parameter values to the key if KeyParameters is specified
        if (!string.IsNullOrEmpty(KeyParameters))
        {
            var paramNames = KeyParameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(p => p.Trim().ToLowerInvariant())
                                          .ToList();

            var parameterValues = new List<string>();
            foreach (var paramName in paramNames)
            {
                // Try to get value from route data
                if (filterContext.RouteData.Values.ContainsKey(paramName))
                {
                    parameterValues.Add(filterContext.RouteData.Values[paramName]?.ToString());
                }
                // Try to get value from form data or query string
                else if (request.Params.AllKeys.Contains(paramName, StringComparer.OrdinalIgnoreCase))
                {
                    parameterValues.Add(request.Params[paramName]?.ToString());
                }
                // Check if it's in the action arguments dictionary (e.g., from model binding)
                else if (filterContext.ActionParameters.ContainsKey(paramName))
                {
                    parameterValues.Add(filterContext.ActionParameters[paramName]?.ToString());
                }
                else
                {
                    // If parameter not found, maybe log it or decide how to handle
                    parameterValues.Add("[NotFound]"); // Indicate parameter was requested but not found
                }
            }

            // Combine parameter values into the key
            finalCacheKey = $"{cacheKeyBase}_{string.Join("_", parameterValues)}";
        }


        // --- Rest of the throttling logic (same as before) ---
        var currentCount = (int?)cache.Get(finalCacheKey);

        if (currentCount == null)
        {
            cache.Add(finalCacheKey, 1, DateTimeOffset.Now.AddMinutes(TimeWindowMinutes));
        }
        else if (currentCount < MaxRequests)
        {
            cache.Set(finalCacheKey, currentCount + 1, DateTimeOffset.Now.AddMinutes(TimeWindowMinutes));
        }
        else
        {
            filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.ServiceUnavailable, "Too many requests. Please try again later.");
            filterContext.HttpContext.Response.Headers.Add("Retry-After", (TimeWindowMinutes * 60).ToString());
            return;
        }

        base.OnActionExecuting(filterContext);
    }
}