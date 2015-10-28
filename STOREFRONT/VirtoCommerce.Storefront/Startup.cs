﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Practices.Unity;
using Owin;
using VirtoCommerce.Client;
using VirtoCommerce.Client.Api;
using VirtoCommerce.LiquidThemeEngine;
using VirtoCommerce.Storefront;
using VirtoCommerce.Storefront.App_Start;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Owin;

[assembly: OwinStartup(typeof(Startup))]
[assembly: PreApplicationStartMethod(typeof(Startup), "PreApplicationStart")]

namespace VirtoCommerce.Storefront
{
    public partial class Startup
    {
        private static readonly List<string> _directories = new List<string>(new[] { Path.Combine(HostingEnvironment.MapPath("~/App_Data"), Environment.Is64BitProcess ? "x64" : "x86") });
        private static Assembly _managerAssembly;

        public static void PreApplicationStart()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;

            var managerAssemblyPath = HostingEnvironment.MapPath("~/Areas/Admin/bin/VirtoCommerce.Platform.Web.dll");
            if (managerAssemblyPath != null && File.Exists(managerAssemblyPath))
            {
                _directories.Add(HostingEnvironment.MapPath("~/Areas/Admin/bin"));

                _managerAssembly = Assembly.LoadFrom(managerAssemblyPath);
                BuildManager.AddReferencedAssembly(_managerAssembly);
            }
        }

        public void Configuration(IAppBuilder app)
        {

            UnityWebActivator.Start();
            var container = UnityConfig.GetConfiguredContainer();
            container.RegisterInstance<WorkContext>(new WorkContext());
            var apiClient = new HmacApiClient(ConfigurationManager.ConnectionStrings["VirtoCommerceBaseUrl"].ConnectionString, ConfigurationManager.AppSettings["vc-public-ApiAppId"], ConfigurationManager.AppSettings["vc-public-ApiSecretKey"]);
            container.RegisterType<IStoreModuleApi, StoreModuleApi>(new InjectionConstructor(apiClient));
            container.RegisterType<IVirtoCommercePlatformApi, VirtoCommercePlatformApi>(new InjectionConstructor(apiClient));
            container.RegisterType<ICustomerManagementModuleApi, CustomerManagementModuleApi>(new InjectionConstructor(apiClient));
            container.RegisterType<ICommerceCoreModuleApi, CommerceCoreModuleApi>(new InjectionConstructor(apiClient));

            container.RegisterType<IStorefrontUrlBuilder, StorefrontUrlBuilder>();
            if (_managerAssembly != null)
            {
                AreaRegistration.RegisterAllAreas();
                CallChildConfigure(app, _managerAssembly, "VirtoCommerce.Platform.Web.Startup", "Configuration", "~/areas/admin", "admin/");
            }

            container.RegisterInstance<ShopifyLiquidThemeStructure>(new ShopifyLiquidThemeStructure(() => { return container.Resolve<WorkContext>(); }, container.Resolve<IStorefrontUrlBuilder>(), "~/App_data/themes", "~/themes/assets"));
            //Register liquid engine
            ViewEngines.Engines.Add(new DotLiquidThemedViewEngine(container.Resolve<ShopifyLiquidThemeStructure>()));
            //FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes, container.Resolve<ICommerceCoreModuleApi>());
            AuthConfig.ConfigureAuth(app);

       
            // Create new work context per each request
            // TODO: Add caching
            app.CreatePerOwinContext(() => container.Resolve<WorkContext>());

            app.UseWorkContext(container.Resolve<WorkContextOptions>());
        }


        private static void CallChildConfigure(IAppBuilder app, Assembly assembly, string typeName, string methodName, string virtualRoot, string routPrefix)
        {
            var type = assembly.GetType(typeName);
            if (type != null)
            {
                var methodInfo = type.GetMethod(methodName, new[] { typeof(IAppBuilder), typeof(string), typeof(string) });
                if (methodInfo != null)
                {
                    var classInstance = Activator.CreateInstance(type, null);
                    var parameters = new object[] { app, virtualRoot, routPrefix };
                    var result = methodInfo.Invoke(classInstance, parameters);
                }
            }
        }

        private static Assembly Resolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null;

            var assemblyName = new AssemblyName(args.Name);
            var fileName = assemblyName.Name + ".dll";

            foreach (var directoryPath in _directories)
            {
                var filePath = Path.Combine(directoryPath, fileName);
                if (File.Exists(filePath))
                {
                    assembly = Assembly.LoadFrom(filePath);
                    break;
                }
            }

            return assembly;
        }
    }
}
