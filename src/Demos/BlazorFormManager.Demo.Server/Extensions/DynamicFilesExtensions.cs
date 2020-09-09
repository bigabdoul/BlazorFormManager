using BlazorFormManager.Demo.Server.Services;
using Microsoft.AspNetCore.Builder;

namespace BlazorFormManager.Demo.Server.Extensions
{
    public static class DynamicFilesExtensions
    {

        /// <summary>
        /// Enables dynamic file serving for the current request path.
        /// </summary>
        /// <param name="app"></param>
        public static void UseDynamicFiles(this IApplicationBuilder app)
        {
            app.UseMiddleware<DynamicFilesMiddleware>();
        }
    }
}
