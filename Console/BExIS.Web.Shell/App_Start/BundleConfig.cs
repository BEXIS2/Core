using System.Web.Optimization;

namespace BExIS.Web.Shell
{
    public class BundleConfig
    {
        // Weitere Informationen zu Bundling finden Sie unter "http://go.microsoft.com/fwlink/?LinkId=301862"
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-2.2.4.min.js",
                "~/Scripts/jquery-migrate-1.2.1.min.js",
                "~/Scripts/jQueryUI/1.10.3/js/jquery-ui-1.10.3.custom.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate.min.js",
                "~/Scripts/jquery.validate.unobtrusive.min.js",
                "~/Scripts/jquery.unobtrusive-ajax.min.js"));

            // Verwenden Sie die Entwicklungsversion von Modernizr zum Entwickeln und Erweitern Ihrer Kenntnisse. Wenn Sie dann
            // für die Produktion bereit sind, verwenden Sie das Buildtool unter "http://modernizr.com", um nur die benötigten Tests auszuwählen.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.min.js",
                "~/Scripts/bootstrap-slider.min.js",
                "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/telerik").Include(
                "~/Scripts/2013.2.611/telerik.all.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bexis").Include(
                "~/Scripts/bexis_default.js",
                "~/Scripts/bexis-jquery-ui.js"));

            bundles.Add(new ScriptBundle("~/bundles/handsontable").Include(
                "~/Scripts/handsontable/handsontable.full.js"));

            bundles.Add(new ScriptBundle("~/bundles/autosize").Include(
                "~/Scripts/autosize/autosize.js"));

            bundles.Add(new ScriptBundle("~/bundles/minimap").Include(
                "~/Scripts/minimap/minimap.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/switchery").Include(
                "~/Scripts/switchery/switchery.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                "~/Scripts/DataTables/media/js/jquery.dataTables.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                //$"~/{Themes.GetResourcePath("Styles")}/bexis-jquery-ui.css",
                //$"~/{Themes.GetResourcePath("Styles")}/bexis-font-awesome-extension.css",
                //$"~/{Themes.GetResourcePath("Styles")}/bexis-elements.css",
                //$"~/{Themes.GetResourcePath("Styles")}/bexis-telerik.css",
                //$"~/{Themes.GetResourcePath("Styles")}/bexis-metadata.css",
                "~/Scripts/jQueryUI/1.10.3/css/smoothness/jquery-ui-1.10.3.custom.min.css",
                "~/Content/bootstrap.css",
                "~/Scripts/switchery/switchery.css",
                "~/Content/bootstrap-slider.min.css",
                "~/Scripts/minimap/minimap.min.css",
                "~/Content/handsontable/handsontable.full.css",
                "~/Content/DataTables/media/css/jquery.dataTables.css",
                "~/Content/Site.css"));

            bundles.Add(new StyleBundle("~/bundles/general_bexis").Include(

                 "~/Themes/Default/Styles/bexis-custom-style.css",
                 "~/Themes/Default/Styles/bexis-jquery-ui.css",
                 "~/Themes/Default/Styles/bexis-font-awesome-extension.css",
                 "~/Themes/Default/Styles/bexis-elements.css",
                 "~/Themes/Default/Styles/bexis-datatables-net.css",
                 "~/Themes/Default/Styles/bexis-telerik.css"
                ));

            #region sveltelayout

            bundles.Add(new ScriptBundle("~/bundles/svelte_jquery").Include(
                "~/Scripts/jquery-2.2.4.min.js",
                "~/Scripts/jquery-migrate-1.2.1.min.js"));

            bundles.Add(new StyleBundle("~/bundles/svelte_general_bexis").Include(

                 "~/Themes/Default/Styles/bexis-custom-style.css",
                 "~/Themes/Default/Styles/bexis-jquery-ui.css",
                 "~/Themes/Default/Styles/bexis-elements.css"
                ));

            bundles.Add(new StyleBundle("~/Content/svelte_css").Include(
                "~/Content/Site.css"));

            #endregion sveltelayout

            BundleTable.EnableOptimizations = true;
        }
    }
}