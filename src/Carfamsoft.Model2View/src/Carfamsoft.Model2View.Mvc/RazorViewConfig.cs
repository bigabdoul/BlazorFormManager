using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

#if NET45_OR_GREATER
using System.Text.RegularExpressions;
using System.Web.Mvc;
#endif

namespace Carfamsoft.Model2View.Mvc
{
    using Properties;

    /// <summary>
    /// Provides razor view-related extension and utility methods.
    /// </summary>
    public static class RazorViewConfig
    {
        #region fields and properties

        /// <summary>
        /// Returns a dictionary containing the names and contents of all embedded razor views.
        /// </summary>
        public static readonly IEnumerable<KeyValuePair<string, byte[]>> EmbeddedRazorViews =
            new Dictionary<string, byte[]>
            {
                { nameof(Resources.FormDisplayAutoEditForm), Resources.FormDisplayAutoEditForm },
                { nameof(Resources.FormDisplayAutoEdit), Resources.FormDisplayAutoEdit },
                { nameof(Resources.FormDisplayGroup), Resources.FormDisplayGroup },
                { nameof(Resources.FormDisplayGroupIcon), Resources.FormDisplayGroupIcon },
                { nameof(Resources.FormDisplayGroupBody), Resources.FormDisplayGroupBody },
                { nameof(Resources.FormDisplayAutoInput), Resources.FormDisplayAutoInput },
                { nameof(Resources.FormDisplayAutoEntry), Resources.FormDisplayAutoEntry },
                { nameof(Resources.FormDisplayInputGroupEntry), Resources.FormDisplayInputGroupEntry },
                { nameof(Resources.FormDisplayAutoFileInput), Resources.FormDisplayAutoFileInput },
            };


        /// <summary>
        /// Returns the names of all views stored as embedded resources.
        /// </summary>
        public static readonly string[] EmbeddedRazorViewNames = 
            new[]
            {
                nameof(Resources.FormDisplayAutoEditForm),
                nameof(Resources.FormDisplayAutoEdit),
                nameof(Resources.FormDisplayGroup),
                nameof(Resources.FormDisplayGroupIcon),
                nameof(Resources.FormDisplayGroupBody),
                nameof(Resources.FormDisplayAutoInput),
                nameof(Resources.FormDisplayInputGroupEntry),
                nameof(Resources.FormDisplayAutoFileInput),
            };

        /// <summary>
        /// Gets a collection of key/value pairs that specifies the new file names 
        /// under which to save the embedded razor views during shared views registration.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, string>> EmbeddedRazorViewNamesMappings { get; private set; }

        /// <summary>
        /// Gets the name of the partial view used to perform custom rendering.
        /// This is usually the entry-point view used to auto-generate an HTML form from a model.
        /// </summary>
        public static string FormDisplayAutoEditForm
        {
            get => EmbeddedRazorViewNamesMappings?.FirstOrDefault(m => m.Key == nameof(Resources.FormDisplayAutoEditForm)).Value ?? nameof(Resources.FormDisplayAutoEditForm);
        }

        /// <summary>
        /// Gets the name of the partial view used to perform custom rendering.
        /// This is usually the entry-point view used to auto-generate HTML form 
        /// elements from models. This view doesn't render the form, only its elements.
        /// </summary>
        public static string FormDisplayAutoEdit
        {
            get => EmbeddedRazorViewNamesMappings?.FirstOrDefault(m => m.Key == nameof(Resources.FormDisplayAutoEdit)).Value ?? nameof(Resources.FormDisplayAutoEdit);
        }

        /// <summary>
        /// Gets the name of the partial view used to perform custom rendering.
        /// </summary>
        public static string FormDisplayGroup
        {
            get => EmbeddedRazorViewNamesMappings?.FirstOrDefault(m => m.Key == nameof(Resources.FormDisplayGroup)).Value ?? nameof(Resources.FormDisplayGroup);
        }

        /// <summary>
        /// Gets the name of the partial view used to render a display group's icon.
        /// </summary>
        public static string FormDisplayGroupIcon
        {
            get => EmbeddedRazorViewNamesMappings?.FirstOrDefault(m => m.Key == nameof(Resources.FormDisplayGroupIcon)).Value ?? nameof(Resources.FormDisplayGroupIcon);
        }

        /// <summary>
        /// Gets the name of the partial view used to perform custom rendering.
        /// </summary>
        public static string FormDisplayGroupBody
        {
            get => EmbeddedRazorViewNamesMappings?.FirstOrDefault(m => m.Key == nameof(Resources.FormDisplayGroupBody)).Value ?? nameof(Resources.FormDisplayGroupBody);
        }

        /// <summary>
        /// Gets the name of the partial view used to auto-build the render tree.
        /// </summary>
        public static string FormDisplayAutoInput
        {
            get => EmbeddedRazorViewNamesMappings?.FirstOrDefault(m => m.Key == nameof(Resources.FormDisplayAutoInput)).Value ?? nameof(Resources.FormDisplayAutoInput);
        }

        /// <summary>
        /// Gets the name of the partial view used to auto-build the render tree 
        /// using the latest CSS framework version.
        /// </summary>
        public static string FormDisplayAutoEntry
        {
            get => EmbeddedRazorViewNamesMappings?.FirstOrDefault(m => m.Key == nameof(Resources.FormDisplayAutoEntry)).Value ?? nameof(Resources.FormDisplayAutoEntry);
        }

        /// <summary>
        /// Gets the name of the partial view used to render an input-group.
        /// </summary>
        public static string FormDisplayInputGroupEntry
        {
            get => EmbeddedRazorViewNamesMappings?.FirstOrDefault(m => m.Key == nameof(Resources.FormDisplayInputGroupEntry)).Value ?? nameof(Resources.FormDisplayInputGroupEntry);
        }

        /// <summary>
        /// Gets the name of the partial view used to render an auto-input file.
        /// </summary>
        public static string FormDisplayAutoFileInput
        {
            get => EmbeddedRazorViewNamesMappings?.FirstOrDefault(m => m.Key == nameof(Resources.FormDisplayAutoFileInput)).Value ?? nameof(Resources.FormDisplayAutoFileInput);
        }

        #endregion

        #region methods

        /// <summary>
        /// Returns the contents of an embedded resource as a byte array.
        /// </summary>
        /// <param name="name">The name of the resource to retrieve.</param>
        /// <param name="contents">Returns the contents of the resource.</param>
        /// <returns>true if the resource exists and was found; otherwise false.</returns>
        public static bool GetEmbeddedResource(string name, out byte[] contents)
            => (contents = Resources.ResourceManager.GetObject(name) as byte[]) != null;

        /// <summary>
        /// Generates (from embedded resources) razor views that support auto-rendering 
        /// HTML components. The views are created in the <paramref name="physicalDirectory"/>. 
        /// Existing views are replaced only if <paramref name="overwrite"/> is true.
        /// This method should be called during application start up, usually within a 
        /// 'Global.asax' file or a 'Startup' class.
        /// </summary>
        /// <param name="physicalDirectory">The physical directory in which views are to be created.</param>
        /// <param name="embeddedRazorViewNamesMappings">
        /// A collection of key/value pairs that specifies the new file names under which to save the embedded razor views.
        /// </param>
        public static void RegisterSharedViews(string physicalDirectory, IEnumerable<KeyValuePair<string, string>> embeddedRazorViewNamesMappings = null)
            => RegisterSharedViews(physicalDirectory, overwrite: false, embeddedRazorViewNamesMappings);

        /// <summary>
        /// Generates (from embedded resources) razor views that support auto-rendering 
        /// HTML components. The views are created in the <paramref name="physicalDirectory"/>. 
        /// Existing views are replaced only if <paramref name="overwrite"/> is true.
        /// This method should be called during application start up, usually within a 
        /// 'Global.asax' file or a 'Startup' class.
        /// </summary>
        /// <param name="physicalDirectory">The physical directory in which views are to be created.</param>
        /// <param name="overwrite">true to overwrite any existing file, otherwise false.</param>
        /// <param name="embeddedRazorViewNamesMappings">
        /// A collection of key/value pairs that specifies the new file names under which to save the embedded razor views.
        /// </param>
        public static void RegisterSharedViews(string physicalDirectory, bool overwrite, IEnumerable<KeyValuePair<string, string>> embeddedRazorViewNamesMappings = null)
        {
            RegisterEmbeddedRazorViews
            (
                physicalDirectory,
                overwrite,
                embeddedRazorViewNamesMappings,
                EmbeddedRazorViewNames
            );
        }

        /// <summary>
        /// Retrieves the specified resources stored as a byte array and writes
        /// them to files as razor views with corresponding names in the 
        /// virtual '~/Views/Shared' folder of the current application.
        /// </summary>
        /// <param name="physicalDirectory">The physical directory in which views are to be created.</param>
        /// <param name="overwrite">true to overwrite any existing file, otherwise false.</param>
        /// <param name="resourceNames">A one-dimensional array of resource names to create as shared razor views.</param>
        public static void RegisterEmbeddedRazorViews(string physicalDirectory, bool overwrite, params string[] resourceNames)
        {
            RegisterEmbeddedRazorViews(physicalDirectory, overwrite, embeddedRazorViewNamesMappings: null, resourceNames);
        }

        /// <summary>
        /// Retrieves the specified resources stored as a byte array and writes
        /// them to files as razor views with corresponding names in the 
        /// virtual '~/Views/Shared' folder of the current application.
        /// </summary>
        /// <param name="physicalDirectory">The physical directory in which views are to be created.</param>
        /// <param name="overwrite">true to overwrite any existing file, otherwise false.</param>
        /// <param name="embeddedRazorViewNamesMappings"></param>
        /// <param name="resourceNames">A one-dimensional array of resource names to create as shared razor views.</param>
        /// <exception cref="NotSupportedException">A specified resource name does not exist or is not a byte array.</exception>
        public static void RegisterEmbeddedRazorViews(string physicalDirectory, bool overwrite, IEnumerable<KeyValuePair<string, string>> embeddedRazorViewNamesMappings, params string[] resourceNames)
        {
            foreach (var viewName in resourceNames)
            {
                if (GetEmbeddedResource(viewName, out byte[] contents))
                {
                    var physicalPath = Path.Combine(physicalDirectory, $"{GetMappedViewName()}.cshtml");
                    WriteAllBytes(physicalPath, contents, overwrite);
                }
                else
                {
                    throw new NotSupportedException($"The resource '{viewName}' does not exist or is not a byte array.");
                }

                string GetMappedViewName()
                {
                    if (embeddedRazorViewNamesMappings == null) return viewName;
                    return embeddedRazorViewNamesMappings.FirstOrDefault(m => m.Key == viewName).Value ?? viewName;
                }
            }

            if (EmbeddedRazorViewNamesMappings == null)
                EmbeddedRazorViewNamesMappings = embeddedRazorViewNamesMappings;
        }

        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes
        /// the file. If the target file already exists, it is overwritten if allowed.
        /// </summary>
        /// <param name="physicalPath">The file to write to.</param>
        /// <param name="contents">The bytes to write to the file.</param>
        /// <param name="overwrite">true to overwrite any existing file, otherwise false.</param>
        /// <returns>true if the <paramref name="contents"/> has been written, otherwise false.</returns>
        public static bool WriteAllBytes(string physicalPath, byte[] contents, bool overwrite = false)
        {
            if (contents != null && (overwrite || !File.Exists(physicalPath)))
            {
                File.WriteAllBytes(physicalPath, contents);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file, and then closes
        /// the file. If the target file already exists, it is overwritten if allowed.
        /// </summary>
        /// <param name="physicalPath">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="overwrite">true to overwrite any existing file, otherwise false.</param>
        /// <returns>true if the <paramref name="contents"/> has been written, otherwise false.</returns>
        public static bool WriteAllText(string physicalPath, string contents, bool overwrite = false)
        {
            if (!string.IsNullOrWhiteSpace(contents) && (overwrite || !File.Exists(physicalPath)))
            {
                File.WriteAllText(physicalPath, contents);
                return true;
            }
            return false;
        }

#if NET45_OR_GREATER
        /// <summary>
        /// Generates (from embedded resources) razor views that support auto-rendering 
        /// HTML components. The views are created in the '~/Views/Shared' folder. 
        /// Existing views are replaced only if <paramref name="overwrite"/> is true.
        /// This method should be called during application start up, usually within a 
        /// 'Global.asax' file or a 'Startup' class.
        /// </summary>
        /// <param name="overwrite">true to overwrite any existing file, otherwise false.</param>
        /// <param name="embeddedRazorViewNamesMappings">
        /// A collection of key/value pairs that specifies the new file names under which to save the embedded razor views.
        /// </param>
        public static void RegisterSharedViews(bool overwrite = false, IEnumerable<KeyValuePair<string, string>> embeddedRazorViewNamesMappings = null)
        {
            RegisterSharedViews(System.Web.HttpContext.Current.Server.MapPath("~/Views/Shared"), overwrite, embeddedRazorViewNamesMappings);
        }

        /// <summary>
        /// Renders a view as an HTML string.
        /// </summary>
        /// <param name="context">The controller context.</param>
        /// <param name="viewPath">The view path.</param>
        /// <param name="model">The view model.</param>
        /// <param name="isPartial">true to find a partial view, otherwise false to find a view.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">View cannot be found.</exception>
        /// <exception cref="InvalidOperationException">View or master view not found.</exception>
        public static string RenderViewAsString(ControllerContext context, string viewPath, object model = null, bool isPartial = false)
        {
            // first find the ViewEngine for this view
            var viewEngineResult = isPartial
                ? ViewEngines.Engines.FindPartialView(context, viewPath)
                : ViewEngines.Engines.FindView(context, viewPath, null);

            if (viewEngineResult == null)
                throw new FileNotFoundException("View cannot be found.");

            if (viewEngineResult.View == null)
            {
                throw new InvalidOperationException($"The view '{viewPath}'" +
                    $" or its master was not found, searched locations:{Environment.NewLine}" +
                    string.Join(Environment.NewLine, viewEngineResult.SearchedLocations));
            }

            return context.RenderViewAsString(viewEngineResult, model);
        }

        /// <summary>
        /// Renders a view as an HTML string.
        /// </summary>
        /// <param name="context">The controller context.</param>
        /// <param name="viewEngineResult">The view engine result.</param>
        /// <param name="model">The view model.</param>
        /// <returns></returns>
        public static string RenderViewAsString(this ControllerContext context, ViewEngineResult viewEngineResult, object model)
        {
            context.Controller.ViewData.Model = model;

            using var sw = new StringWriter();
            var viewContext = new ViewContext(context, viewEngineResult.View, context.Controller.ViewData, context.Controller.TempData, sw);
            viewEngineResult.View.Render(viewContext, sw);

            var request = context.HttpContext.Request;
            var html = sw.GetStringBuilder().ToString();
            var baseUrl = string.Format("{0}://{1}", request.Url.Scheme, request.Url.Authority);

            return Regex.Replace(html, "<head>", string.Format("<head><base href=\"{0}\" />", baseUrl), RegexOptions.IgnoreCase);
        }
#endif

        #endregion
    }
}