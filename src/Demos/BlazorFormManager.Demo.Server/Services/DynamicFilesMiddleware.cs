using BlazorFormManager.Demo.Server.Controllers;
using BlazorFormManager.Demo.Server.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorFormManager.Demo.Server.Services
{
    public sealed class DynamicFilesMiddleware
    {
        private readonly RequestDelegate _next;
        private const StringComparison IgnoreCase = StringComparison.OrdinalIgnoreCase;
        private const BindingFlags InvocationFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        public DynamicFilesMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Value.StartsWith("/dynamic", IgnoreCase))
            {
                var (success, controller, action, id) = ExtractControllerInfo(context);

                if (!success)
                {
                    await _next(context);
                    return;
                }

                var result = await context.InvokeControllerActionAsync<FileContentResult>(controller, action, id);

                if (result != null)
                {
                    await context.Response.Body.WriteAsync(result.FileContents);
                    return;
                }
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }

        private (bool success, string controller, string action, string id) ExtractControllerInfo(HttpContext context)
        {
            // sample request path pattern: /dynamic/account/photo/1045.jpg
            var parts = context.Request.Path.Value.Split('/');
            if (parts.Length > 3)
            {
                var controllerName = $"{parts[2][0]}".ToUpper() + $"{parts[2][1..]}Controller"; // AccountController
                var actionName = parts[3]; // photo
                string actionId = parts.Length > 4 ? parts[4] : string.Empty; // 1045.jpg

                if (actionId.EndsWith(".jpg", IgnoreCase)) actionId = actionId[0..^4]; // actionId.Substring(0, actionId.Length - 4);

                controllerName = $"{typeof(AccountController).Namespace}.{controllerName}";
                return (true, controllerName, actionName, actionId);
            }
            return (default, default, default, default);
        }
    }
}
