using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AutoInputViewsDemo
{
    using static Carfamsoft.Model2View.Mvc.RazorViewConfig;
    
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            RegisterSharedViews();
        }

        static void RegisterSharedViews()
        {
            // We could register the shared views using their default names with this one-liner:
            // Carfamsoft.Model2View.Mvc.RazorViewConfig.RegisterSharedViews(HttpContext.Current.Server.MapPath("~/Views/Shared"));

            // Mapping the default view names to configurable custom names is more
            // flexible and avoids possible naming conflicts in existing projects.
            const string keyPrefix = "RazorViewConfig:SharedViews.";
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            var mappings = new System.Collections.Generic.Dictionary<string, string>
            {
                { nameof(FormDisplayAutoEditForm), appSettings[$"{keyPrefix}{nameof(FormDisplayAutoEditForm)}"] },
                { nameof(FormDisplayAutoEdit), appSettings[$"{keyPrefix}{nameof(FormDisplayAutoEdit)}"] },
                { nameof(FormDisplayGroup), appSettings[$"{keyPrefix}{nameof(FormDisplayGroup)}"] },
                { nameof(FormDisplayGroupIcon), appSettings[$"{keyPrefix}{nameof(FormDisplayGroupIcon)}"] },
                { nameof(FormDisplayGroupBody), appSettings[$"{keyPrefix}{nameof(FormDisplayGroupBody)}"] },
                { nameof(FormDisplayAutoInput), appSettings[$"{keyPrefix}{nameof(FormDisplayAutoInput)}"] },
                { nameof(FormDisplayAutoEntry), appSettings[$"{keyPrefix}{nameof(FormDisplayAutoEntry)}"] },
                { nameof(FormDisplayInputGroupEntry), appSettings[$"{keyPrefix}{nameof(FormDisplayInputGroupEntry)}"] },
                { nameof(FormDisplayAutoFileInput), appSettings[$"{keyPrefix}{nameof(FormDisplayAutoFileInput)}"] },
            };

            Carfamsoft.Model2View.Mvc.RazorViewConfig.RegisterSharedViews(HttpContext.Current.Server.MapPath("~/Views/Shared"), mappings);
        }
    }
}
