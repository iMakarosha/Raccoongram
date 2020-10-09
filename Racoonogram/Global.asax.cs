using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Configuration;
using System.Web.Routing;

namespace Racoonogram
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        //public override void Init()
        //{
        //    base.Init();
        //    this.BeginRequest += GlobalBeginRequest;
        //}

        //private void GlobalBeginRequest(object sender, EventArgs e)
        //{
        //    var runTime = (HttpRuntimeSection)WebConfigurationManager.GetSection("system.web/httpRuntime");
        //    var maxRequestLength = runTime.MaxRequestLength * 1024;

        //    if (Request.ContentLength > maxRequestLength)
        //    {
        //        // или другой свой код обработки
        //        // Response.Redirect("~/Controllers/HomeController.cs/index");
        //       // HttpContext.Current.RewritePath("/Home/Index");
        //        var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
        //        Response.Redirect(urlHelper.Action("Index", "Home"));
        //    }
        //}

    }
}
