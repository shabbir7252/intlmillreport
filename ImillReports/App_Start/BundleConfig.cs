using System.Web;
using System.Web.Optimization;

namespace ImillReports
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/ReportScript").Include(
                "~/Scripts/libscripts.bundle.js",
                "~/Scripts/vendorscripts.bundle.js",
                //"~/Scripts/morrisscripts.bundle.js",
                "~/Scripts/mainscripts.bundle.js",
                "~/Scripts/index2.js",
                "~/Scripts/chart.min.js",
                "~/Scripts/site.js"
                // "~/Scripts/ej2/ej2.min.js"
                ));

            bundles.Add(new StyleBundle("~/bundles/Bootstrap").Include(
                "~/Content/stylesheets/bootstrap.min.css"
                ));

            bundles.Add(new StyleBundle("~/bundles/ReportStyles").Include(
                // "~/Content/stylesheets/bootstrap.min.css",
                // "~/Content/stylesheets/morris.css",
                 // "~/Content/stylesheets/main.css",
                "~/Content/stylesheets/hm-style.css",
                // "~/Content/stylesheets/color_skins.css",
                "~/Content/ej2/material.css",
                "~/Content/stylesheets/site.css"
                ));
        }
    }
}
