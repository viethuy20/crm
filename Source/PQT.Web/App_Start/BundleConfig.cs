using System.Web.Optimization;
using NS;
using NS.Mvc.Helpers;

namespace PQT.Web
{
    public class
        BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new StyleBundle("~/content/css/min").Include(
                "~/Content/css/theme-default/bootstrap.css"
                , "~/Content/css/theme-default/font-awesome.min.css"
                , "~/Content/css/theme-default/material-design-iconic-font.min.css"
                , "~/Content/css/theme-default/libs/rickshaw/rickshaw.css"
                , "~/Content/css/theme-default/libs/morris/morris.core.css"
                , "~/Content/css/theme-default/libs/select2/select2-4.0.1.css"
                , "~/Content/css/theme-default/libs/select2/select2.css"
                //, "~/Content/js/libs/select2-4.0.1/css/select2.css"
                , "~/Content/css/theme-default/libs/multi-select/multi-select.css"
                , "~/Content/css/theme-default/libs/bootstrap-datepicker/datepicker3.css"
                , "~/Content/css/theme-default/libs/jquery-ui/jquery-ui-theme.css"
                , "~/Content/css/theme-default/libs/bootstrap-colorpicker/bootstrap-colorpicker.css"
                , "~/Content/css/theme-default/libs/bootstrap-tagsinput/bootstrap-tagsinput.css"
                , "~/Content/css/theme-default/libs/DataTables/jquery.dataTables.css"
                //, "~/Content/css/theme-default/libs/DataTables/extensions/dataTables.colVis.css"
                //, "~/Content/css/theme-default/libs/DataTables/extensions/dataTables.tableTools.css"
                , "~/Content/js/libs/DataTables/extensions/FixedColumns/css/dataTables.fixedColumns.min.css"
                //, "~/Content/js/libs/DataTables-1.10.0/media/css/jquery.dataTables.css"
                //, "~/Content/js/libs/DataTables-1.10.0/extensions/FixedHeader/css/dataTables.fixedHeader.min.css"
                //, "~/Content/js/libs/DataTables-1.10.0/extensions/FixedColumns/css/dataTables.fixedColumns.min.css"
                , "~/Content/js/magnific-popup/magnific-popup.css"
                , "~/Content/css/theme-default/libs/rickshaw/rickshaw.css"
                , "~/Content/css/theme-default/libs/morris/morris.core.css"
                , "~/Content/css/theme-default/libs/toastr/toastr.min.css"
                , "~/Content/css/theme-default/libs/wizard/wizard.css"
                , "~/Content/css/theme-default/materialadmin.css"
                ));
            bundles.Add(new JsBundle("~/content/js/min").Include(

                 "~/Content/js/libs/jquery/jquery-1.11.2.min.js"
                , "~/Content/js/libs/jquery/jquery-migrate-1.2.1.min.js"
                //, "~/Content/js/libs/jquery-ui/jquery-ui.min.js"
                , "~/Content/js/libs/bootstrap/bootstrap.min.js"
                , "~/Content/js/libs/spin.js/spin.min.js"
                , "~/Content/js/libs/autosize/jquery.autosize.min.js"
                //, "~/Content/js/libs/select2/select2.min.js"
                , "~/Content/js/libs/select2-4.0.1/js/select2.full.min.js"
                , "~/Content/js/libs/bootstrap-tagsinput/bootstrap-tagsinput.min.js"
                , "~/Content/js/libs/multi-select/jquery.multi-select.js"
                , "~/Content/js/libs/inputmask/jquery.inputmask.bundle.min.js"
                , "~/Content/js/libs/moment/moment.min.js"
                , "~/Content/js/libs/bootstrap-datepicker/bootstrap-datepicker.js"
                , "~/Content/js/libs/bootstrap-colorpicker/bootstrap-colorpicker.min.js"
                , "~/Content/js/libs/wizard/jquery.bootstrap.wizard.min.js"
                //, "~/Content/js/libs/DataTables/jquery.dataTables.min.js"
                //, "~/Content/js/libs/DataTables/extensions/ColVis/js/dataTables.colVis.min.js"
                //, "~/Content/js/libs/DataTables/extensions/TableTools/js/dataTables.tableTools.min.js"
                //, "~/Content/js/libs/DataTables/extensions/FixedColumns/js/dataTables.fixedColumns.min.js"
                , "~/Content/js/libs/datatable/datatable.js"
                , "~/Content/js/libs/datatable/dataTables.fixedHeader.min.js"
                , "~/Content/js/libs/DataTables-1.10.0/extensions/FixedColumns/js/dataTables.fixedColumns.min.js"
                , "~/Content/js/libs/flot/jquery.flot.min.js"
                , "~/Content/js/libs/flot/jquery.flot.time.min.js"
                , "~/Content/js/libs/flot/jquery.flot.resize.min.js"
                , "~/Content/js/libs/flot/jquery.flot.orderBars.js"
                , "~/Content/js/libs/flot/jquery.flot.pie.js"
                , "~/Content/js/libs/flot/curvedLines.js"
                , "~/Content/js/libs/jquery-knob/jquery.knob.min.js"
                , "~/Content/js/libs/sparkline/jquery.sparkline.min.js"
                , "~/Content/js/libs/nanoscroller/jquery.nanoscroller.min.js"
                , "~/Content/js/libs/d3/d3.min.js"
                , "~/Content/js/libs/d3/d3.v3.js"
                , "~/Content/js/libs/rickshaw/rickshaw.min.js"
                , "~/Content/js/libs/toastr/toastr.min.js"
                , "~/Content/js/core/source/App.js"
                , "~/Content/js/core/source/AppNavigation.js"
                , "~/Content/js/core/source/AppOffcanvas.js"
                , "~/Content/js/core/source/AppCard.js"
                , "~/Content/js/core/source/AppForm.js"
                , "~/Content/js/core/source/AppNavSearch.js"
                , "~/Content/js/core/source/AppVendor.js"
                , "~/Content/js/core/demo/Demo.js"
                , "~/Content/js/core/demo/DemoFormComponents.js?v=2"
                , "~/Content/js/core/demo/DemoTableDynamic.js"

                
                , "~/Content/js/magnific-popup/jquery.magnific-popup.js"
                , "~/Content/js/ultilities/LINQ_JS.js"
                , "~/Content/js/ultilities/string.js"
                , "~/Content/js/accounting/accounting.js"
                , "~/Content/js/bootbox/bootbox.min.js"
                , "~/Scripts/jQuery.tmpl.js"
                , "~/Scripts/jquery.validate.js"
                , "~/Scripts/jquery.validate.unobtrusive.js"
                , "~/Scripts/jquery.unobtrusive-ajax.js"
                , "~/Scripts/jquery.signalR-{version}.js"
                , "~/Content/js/cldrjs-0.4.4/dist/cldr.js"
                , "~/Content/js/cldrjs-0.4.4/dist/cldr/event.js"
                , "~/Content/js/cldrjs-0.4.4/dist/cldr/supplemental.js"
                , "~/Content/js/globalize-1.1.1/dist/globalize.js"
                , "~/Content/js/globalize-1.1.1/dist/globalize/number.js"
                , "~/Content/js/globalize-1.1.1/dist/globalize/date.js"

                ));


            bundles.Add(new CssBundle("~/css/ie8").Include("~/Display/css/ie.css"));

            bundles.Add(new JsBundle("~/js/ie9").Include(
                "~/Display/js/ie/html5.js"
                , "~/Display/js/ie/respond.min.js"
                , "~/Display/lib/flot/excanvas.min.js"));
        }
    }
}
