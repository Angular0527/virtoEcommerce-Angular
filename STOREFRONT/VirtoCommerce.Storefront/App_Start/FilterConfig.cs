﻿using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.Storefront
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters, Func<WorkContext> workContextFactory)
        {
            filters.Add(new JsonNetActionFilter());
            filters.Add(new StorefrontValidationActionFilter { WorkContextFactory = workContextFactory });
        }
    }
    /// <summary>
    /// This filter сhecks stores and showed special page if any store found
    /// </summary>
    public class StorefrontValidationActionFilter : ActionFilterAttribute
    {
        public Func<WorkContext> WorkContextFactory { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (WorkContextFactory != null)
            {
                var workContext = WorkContextFactory();

                // RequestUrl is null when current request is not a storefront request
                if (workContext != null && workContext.RequestUrl != null
                    && (workContext.AllStores == null || !workContext.AllStores.Any())
                    && filterContext.ActionDescriptor.ActionName != "NoStore")
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Common", action = "NoStore" }));
                }
            }

            base.OnActionExecuting(filterContext);
        }

    }

    public class JsonNetActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var jsonResult = filterContext.Result as JsonResult;

            if (jsonResult != null)
            {
                filterContext.Result = new JsonNetResult
                {
                    ContentEncoding = jsonResult.ContentEncoding,
                    ContentType = jsonResult.ContentType,
                    Data = jsonResult.Data,
                    JsonRequestBehavior = jsonResult.JsonRequestBehavior
                };
            }

            base.OnActionExecuted(filterContext);
        }
    }

    public class JsonNetResult : JsonResult
    {
        public JsonNetResult()
        {
            Settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            Settings.Converters.Add(new StringEnumConverter
            {
                AllowIntegerValues = false,
                CamelCaseText = false
            });
        }

        public JsonSerializerSettings Settings { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var response = context.HttpContext.Response;

            response.ContentType = string.IsNullOrEmpty(ContentType) ? "application/json" : ContentType;

            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }

            if (Data == null)
            {
                return;
            }

            var scriptSerializer = JsonSerializer.Create(Settings);

            scriptSerializer.Serialize(response.Output, Data);
        }
    }
}