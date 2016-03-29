﻿using System;
using System.Data.Entity;
using System.Web;
using System.Web.Http;
using Microsoft.Practices.Unity;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.CustomerModule.Web.Converters;
using VirtoCommerce.CustomerModule.Web.ExportImport;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.CustomerModule.Web
{
    public class Module : ModuleBase, ISupportExportImportModule
    {
        private const string _connectionStringName = "VirtoCommerce";
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void SetupDatabase()
        {
            using (var db = new CustomerRepositoryImpl(_connectionStringName, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<CustomerRepositoryImpl, Data.Migrations.Configuration>();
                initializer.InitializeDatabase(db);
            }

        }

        public override void Initialize()
        {
            _container.RegisterType<ICustomerRepository>(new InjectionFactory(c => new CustomerRepositoryImpl(_connectionStringName, new EntityPrimaryKeyGeneratorInterceptor(), _container.Resolve<AuditableInterceptor>())));
            _container.RegisterType<IMemberServicesFactory, DefaultMembersServiceFactory>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IMemberService, MemberServicesProxy>();

         
        }

        public override void PostInitialize()
        {
            var membersFactory = _container.Resolve<IMemberServicesFactory>();
            //Default memebrs service support Contact, Organization, Vendor and Employee types
            var defaultMemberService = new DefaultMemberService(_container.Resolve<Func<ICustomerRepository>>(),_container.Resolve<IDynamicPropertyService>(), _container.Resolve<ISecurityService>());
        
            membersFactory.RegisterMemberService(defaultMemberService);

            //Next lines allow to use polymorph types in API controller methods
            var formatters = GlobalConfiguration.Configuration.Formatters;
            formatters.JsonFormatter.SerializerSettings.Converters.Add(new PolymorphicMemberJsonConverter(membersFactory));
            formatters.JsonFormatter.SerializerSettings.Converters.Add(new PolymorphicMemberSearchCriteriaJsonConverter());

            base.PostInitialize();
        }
        #endregion

        #region ISupportExportImportModule Members

        public void DoExport(System.IO.Stream outStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var exportJob = _container.Resolve<CustomerExportImport>();
            exportJob.DoExport(outStream, progressCallback);
        }

        public void DoImport(System.IO.Stream inputStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var exportJob = _container.Resolve<CustomerExportImport>();
            exportJob.DoImport(inputStream, progressCallback);
        }

        public string ExportDescription
        {
            get
            {
                var settingManager = _container.Resolve<ISettingsManager>();
                return settingManager.GetValue("Customer.ExportImport.Description", String.Empty);
            }
        }

        #endregion
    }

}
